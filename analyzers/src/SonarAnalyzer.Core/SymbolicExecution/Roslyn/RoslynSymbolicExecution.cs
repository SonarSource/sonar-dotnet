/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
 * mailto: contact AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

using SonarAnalyzer.CFG.LiveVariableAnalysis;
using SonarAnalyzer.CFG.Roslyn;
using SonarAnalyzer.SymbolicExecution.Constraints;
using SonarAnalyzer.SymbolicExecution.Roslyn.Checks;

namespace SonarAnalyzer.SymbolicExecution.Roslyn;

internal class RoslynSymbolicExecution
{
    internal const int MaxStepCount = 4000;
    private const int MaxOperationVisits = 3;

    private readonly ControlFlowGraph cfg;
    private readonly SyntaxClassifierBase syntaxClassifier;
    private readonly SymbolicCheckList checks;
    private readonly CancellationToken cancel;
    private readonly Queue<ExplodedNode> queue = new();
    private readonly HashSet<ExplodedNode> visited = new();
    private readonly RoslynLiveVariableAnalysis lva;
    private readonly DebugLogger logger = new();
    private readonly ExceptionCandidate exceptionCandidate;
    private readonly LoopDetector loopDetector;

    public RoslynSymbolicExecution(ControlFlowGraph cfg, SyntaxClassifierBase syntaxClassifier, SymbolicCheck[] checks, CancellationToken cancel)
    {
        this.cfg = cfg ?? throw new ArgumentNullException(nameof(cfg));
        this.syntaxClassifier = syntaxClassifier ?? throw new ArgumentNullException(nameof(syntaxClassifier));
        if (checks == null || checks.Length == 0)
        {
            throw new ArgumentException("At least one check is expected", nameof(checks));
        }
        this.checks = new(new SymbolicCheck[] { new NonNullableValueTypeCheck(), new ConstantCheck() }.Concat(checks).ToArray());
        this.cancel = cancel;
        exceptionCandidate = new(cfg.OriginalOperation.ToSonar().SemanticModel.Compilation);
        loopDetector = new(cfg);
        lva = new(cfg, cancel);
        logger.Log(cfg);
    }

    public void Execute()
    {
        if (visited.Any())
        {
            throw new InvalidOperationException("Engine can be executed only once.");
        }
        if (!ProgramPoint.HasSupportedSize(cfg))
        {
            return;
        }
        var steps = 0;
        queue.Enqueue(new(cfg.EntryBlock, ProgramState.Empty, null));
        while (queue.Any())
        {
            var current = queue.Dequeue();
            if (visited.Add(current) && CheckVisitCount(current, current.AddVisit()))
            {
                if (steps++ > MaxStepCount || cancel.IsCancellationRequested)
                {
                    return;
                }

                logger.Log(current, "Processing");
                var successors = current.Operation.Instance == null ? ProcessBranching(current) : ProcessOperation(current);
                foreach (var node in successors)
                {
                    logger.Log(node, "Enqueuing", true);
                    queue.Enqueue(node);
                }
            }
            else
            {
                logger.Log(current, "Not visiting");
            }
        }
        logger.Log("Completed");
        checks.ExecutionCompleted();
    }

    private bool CheckVisitCount(ExplodedNode node, int visitCount)
    {
        return visitCount <= MaxOperationVisits
            || (visitCount <= MaxOperationVisits + 1 && IsLoopCondition());

        bool IsLoopCondition() =>
            node.Block.BranchValue is not null
            && (node.Operation.Instance is null || IsInBranchValue(node.Operation.Instance))
            && syntaxClassifier.IsInLoopCondition(node.Block.BranchValue.Syntax);

        bool IsInBranchValue(IOperation current)
        {
            while (current is not null)
            {
                if (current == node.Block.BranchValue)
                {
                    return true;
                }
                current = new IOperationWrapperSonar(current).Parent;
            }
            return false;
        }
    }

    private IEnumerable<ExplodedNode> ProcessBranching(ExplodedNode node)
    {
        if (node.Block.Kind == BasicBlockKind.Exit)
        {
            logger.Log(node.State, "Exit Reached");
            checks.ExitReached(new(node, lva.CapturedVariables, false));
        }
        else if (node.Block.Successors.Length == 1 && ThrownException(node, node.Block.Successors.Single().Semantics) is { } exception)
        {
            foreach (var successor in ExceptionSuccessors(node, exception, node.Block.EnclosingRegion(ControlFlowRegionKind.Try)))
            {
                yield return successor;
            }
        }
        else
        {
            foreach (var successor in node.Block.Successors.Where(x => IsReachable(node, x)))
            {
                if (ProcessBranch(node, successor) is { } newNode)
                {
                    yield return newNode;
                }
            }
        }
    }

    private ExplodedNode ProcessBranch(ExplodedNode node, ControlFlowBranch branch)
    {
        if (branch.Destination is not null)
        {
            return branch.FinallyRegions.Any() // When exiting finally region(s), redirect to 1st finally instead of the normal destination
                ? FromFinally(new FinallyPoint(node.FinallyPoint, branch))
                : CreateNode(branch.Destination, node.FinallyPoint);
        }
        else if (branch.Source.EnclosingRegion.Kind == ControlFlowRegionKind.Finally && node.FinallyPoint is not null)
        {
            return FromFinally(node.FinallyPoint.CreateNext());     // Redirect from finally back to the original place (or outer finally on the same branch)
        }
        else
        {
            if (branch.Source.BranchValue is { } branchValue)
            {
                // If a branch has no Destination but is part of conditional branching we need to call ConditionEvaluated. This happens when a Rethrow is following a condition.
                var state = SetBranchingConstraints(branch, node.State, branchValue);
                checks.ConditionEvaluated(new(node.Block, branchValue.ToSonar(), state, loopDetector.IsInLoop(node.Block), lva.CapturedVariables));
            }
            return null;    // We don't know where to continue
        }

        ExplodedNode FromFinally(FinallyPoint finallyPoint) =>
            CreateNode(cfg.Blocks[finallyPoint.BlockIndex], finallyPoint.IsFinallyBlock ? finallyPoint : finallyPoint.Previous);

        ExplodedNode CreateNode(BasicBlock block, FinallyPoint finallyPoint) =>
            ProcessBranchState(node.Block, branch, node.State) is { } newState ? new(block, newState, finallyPoint) : null;
    }

    private ProgramState ProcessBranchState(BasicBlock block, ControlFlowBranch branch, ProgramState state)
    {
        if (cfg.OriginalOperation.Syntax.Language == LanguageNames.VisualBasic) // Avoid C# FPs as we don't support tuple deconstructions yet
        {
            state = InitLocals(branch, state);
        }
        if (branch.Source.BranchValue is { } branchValue && branch.Source.ConditionalSuccessor is not null) // This branching was conditional
        {
            state = SetBranchingConstraints(branch, state, branchValue);
            SymbolicContext context = new(block, branchValue.ToSonar(), state, loopDetector.IsInLoop(block), lva.CapturedVariables);
            state = checks.ConditionEvaluated(context);
            if (state is null)
            {
                return null;
            }
        }
        foreach (var capture in branch.LeavingRegions.SelectMany(x => x.CaptureIds))
        {
            state = state.RemoveCapture(capture);
        }
        if (state.Exception is not null
            && branch.Source.EnclosingNonLocalLifetimeRegion() is { Kind: ControlFlowRegionKind.Catch or ControlFlowRegionKind.FilterAndHandler } enclosingRegion
            && branch.LeavingRegions.Contains(enclosingRegion))
        {
            state = state.PopException();
        }
        var liveVariables = lva.LiveOut(branch.Source).ToHashSet();
        return state.RemoveSymbols(x => lva.IsLocal(x) && (x is ILocalSymbol or IParameterSymbol { RefKind: RefKind.None }) && !liveVariables.Contains(x))
            .ResetOperations();
    }

    private IEnumerable<ExplodedNode> ProcessOperation(ExplodedNode node)
    {
        foreach (var preProcessed in checks.PreProcess(new(node, lva.CapturedVariables, loopDetector.IsInLoop(node.Block))))
        {
            foreach (var processed in OperationDispatcher.Process(preProcessed))
            {
                foreach (var postProcessed in checks.PostProcess(processed))
                {
                    // When operation doesn't have a parent it is the outer statement. We need to reset operation states:
                    // * We don't need to preserve the inner subexpression intermediate states after the outer statement.
                    // * We don't want ProgramState to contain the path-history data, because we want to avoid exploring the same state twice.
                    // When the operation is a BranchValue, we need to preserve it to evaluate branching. The state will be reset after branching.
                    yield return node.CreateNext(node.Operation.Parent is null && node.Block.BranchValue != node.Operation.Instance ? postProcessed.State.ResetOperations() : postProcessed.State);
                }
            }
        }

        if (exceptionCandidate.FromOperation(node.State, node.Operation) is { } exception && node.Block.EnclosingRegion(ControlFlowRegionKind.Try) is { } tryRegion)
        {
            foreach (var successor in ExceptionSuccessors(node, exception, tryRegion))
            {
                yield return successor;
            }
        }
    }

    private IEnumerable<ExplodedNode> ExceptionSuccessors(ExplodedNode node, ExceptionState exception, ControlFlowRegion nearestTryRegion)
    {
        var state = node.State.ResetOperations();   // We're jumping out of the outer statement => We need to reset operation states.
        state = node.Block.EnclosingRegion(ControlFlowRegionKind.Catch) is not null
            ? state.PushException(exception)        // If we're nested inside catch, we need to preserve the exception chain
            : state.SetException(exception);        // Otherwise we track only the current exception to avoid explosion of states after each statement

        foreach (var handler in nearestTryRegion.ReachableHandlers().Where(x => IsReachable(x, exception)))
        {
            yield return new(cfg.Blocks[handler.FirstBlockOrdinal], state, null);
            if (IsExhaustive(handler))
            {
                yield break;
            }
        }

        yield return new(cfg.ExitBlock, node.State.SetException(exception), null);  // catch without finally or uncaught exception type

        bool IsExhaustive(ControlFlowRegion handler) =>
            handler.Kind switch
            {
                ControlFlowRegionKind.Finally => true,
                ControlFlowRegionKind.FilterAndHandler => false,
                _ => exception.Type.DerivesFrom(handler.ExceptionType)
                    || handler.ExceptionType.IsAny(KnownType.System_Exception, KnownType.System_Object) // relevant for UnkonwnException: 'catch (Exception) { ... }' and 'catch { ... }'
            };
    }

    private static ExceptionState ThrownException(ExplodedNode node, ControlFlowBranchSemantics semantics) =>
        semantics switch
        {
            ControlFlowBranchSemantics.Throw => node.Block.BranchValue.UnwrapConversion().Type is { } type ? new ExceptionState(type) : ExceptionState.UnknownException,
            ControlFlowBranchSemantics.Rethrow => node.State.Exception,
            ControlFlowBranchSemantics.StructuredExceptionHandling when node.FinallyPoint is null => node.State.Exception,  // Exiting 'finally' with exception
            _ => null
        };

    private static ProgramState SetBranchingConstraints(ControlFlowBranch branch, ProgramState state, IOperation branchValue)
    {
        var constraint = BoolConstraint.From((branch.Source.ConditionKind == ControlFlowConditionKind.WhenTrue) == branch.IsConditionalSuccessor);
        state = state.SetOperationConstraint(branchValue, ObjectConstraint.NotNull).SetOperationConstraint(branchValue, constraint);
        return branchValue.TrackedSymbol(state) is { } symbol
            && symbol.GetSymbolType() is { } type
            && (type.SpecialType is SpecialType.System_Boolean || type.IsNullableBoolean())
            ? state.SetSymbolConstraint(symbol, ObjectConstraint.NotNull).SetSymbolConstraint(symbol, constraint)
            : state;
    }

    private static ProgramState InitLocals(ControlFlowBranch branch, ProgramState state)
    {
        foreach (var local in branch.EnteringRegions.SelectMany(x => x.Locals))
        {
            if (ConstantCheck.ConstraintFromType(local.Type) is { } constraint)
            {
                state = state.SetSymbolConstraint(local, constraint);
            }
        }
        return state;
    }

    private static bool IsReachable(ExplodedNode node, ControlFlowBranch branch) =>
        node.Block.ConditionKind == ControlFlowConditionKind.None
        || node.State[node.Block.BranchValue] is not { } symbolicValue
        || !symbolicValue.HasConstraint<BoolConstraint>()
        || IsReachable(branch, node.Block.ConditionKind == ControlFlowConditionKind.WhenTrue, symbolicValue.HasConstraint(BoolConstraint.True));

    private static bool IsReachable(ControlFlowBranch branch, bool condition, bool constraint) =>
        branch.IsConditionalSuccessor ? condition == constraint : condition != constraint;

    private static bool IsReachable(ControlFlowRegion region, ExceptionState thrown) =>
        region.Kind == ControlFlowRegionKind.Finally
        || thrown == ExceptionState.UnknownException
        || thrown.Type.DerivesFrom(region.ExceptionType);
}
