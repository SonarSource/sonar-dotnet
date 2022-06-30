/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.CFG.LiveVariableAnalysis;
using SonarAnalyzer.CFG.Roslyn;
using SonarAnalyzer.Extensions;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.SymbolicExecution.Constraints;
using SonarAnalyzer.SymbolicExecution.Roslyn.Checks;
using SonarAnalyzer.SymbolicExecution.Roslyn.OperationProcessors;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.SymbolicExecution.Roslyn
{
    internal class RoslynSymbolicExecution
    {
        internal const int MaxStepCount = 2000;
        private const int MaxOperationVisits = 2;

        private readonly ControlFlowGraph cfg;
        private readonly SymbolicCheckList checks;
        private readonly Queue<ExplodedNode> queue = new();
        private readonly HashSet<ExplodedNode> visited = new();
        private readonly RoslynLiveVariableAnalysis lva;
        private readonly DebugLogger logger = new();
        private readonly ExceptionCandidate exceptionCandidate;

        public RoslynSymbolicExecution(ControlFlowGraph cfg, SymbolicCheck[] checks)
        {
            this.cfg = cfg ?? throw new ArgumentNullException(nameof(cfg));
            if (checks == null || checks.Length == 0)
            {
                throw new ArgumentException("At least one check is expected", nameof(checks));
            }
            this.checks = new(new[] { new ConstantCheck() }.Concat(checks).ToArray());
            lva = new(cfg);
            exceptionCandidate = new(new IOperationWrapperSonar(cfg.OriginalOperation).SemanticModel.Compilation);
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
                if (steps++ > MaxStepCount)
                {
                    return;
                }
                var current = queue.Dequeue();
                if (visited.Add(current) && current.AddVisit() <= MaxOperationVisits)
                {
                    logger.Log(current, "Processing");
                    var successors = current.Operation == null ? ProcessBranching(current) : ProcessOperation(current);
                    foreach (var node in successors)
                    {
                        logger.Log(node, "Enqueuing", true);
                        queue.Enqueue(node);
                    }
                }
            }
            logger.Log("Completed");
            checks.ExecutionCompleted();
        }

        private IEnumerable<ExplodedNode> ProcessBranching(ExplodedNode node)
        {
            if (node.Block.Kind == BasicBlockKind.Exit)
            {
                logger.Log(node.State, "Exit Reached");
                checks.ExitReached(new(null, node.State));
            }
            else if (node.Block.Successors.Length == 1 && ThrownException(node, node.Block.Successors.Single().Semantics) is { } exception)
            {
                var successors = ExceptionSuccessors(node, exception).ToArray();
                foreach (var successor in successors)
                {
                    yield return successor;
                }

                if (successors.Length == 0) // catch without finally
                {
                    yield return new(cfg.ExitBlock, node.State.SetException(exception), null);
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
            else if (branch.Source.EnclosingRegion.Kind == ControlFlowRegionKind.Finally && node.State.Exception is not null)
            {
                var currentTryAndFinally = branch.Source.EnclosingRegion.EnclosingRegion;
                return currentTryAndFinally.EnclosingRegion(ControlFlowRegionKind.TryAndFinally) is { } outerTryAndFinally
                    ? CreateNode(cfg.Blocks[outerTryAndFinally.NestedRegion(ControlFlowRegionKind.Finally).FirstBlockOrdinal], null)
                    : new(cfg.ExitBlock, node.State, null);
            }
            else
            {
                return null;    // We don't know where to continue
            }

            ExplodedNode FromFinally(FinallyPoint finallyPoint) =>
                CreateNode(cfg.Blocks[finallyPoint.BlockIndex], finallyPoint.IsFinallyBlock ? finallyPoint : finallyPoint.Previous);

            ExplodedNode CreateNode(BasicBlock block, FinallyPoint finallyPoint) =>
                ProcessBranchState(branch, node.State) is { } newState ? new(block, newState, finallyPoint) : null;
        }

        private ProgramState ProcessBranchState(ControlFlowBranch branch, ProgramState state)
        {
            foreach (var local in branch.EnteringRegions.SelectMany(x => x.Locals))
            {
                if (ConstantCheck.ConstraintFromType(local.Type) is { } constraint)
                {
                    state = state.SetSymbolConstraint(local, constraint);
                }
            }
            if (branch.Source.BranchValue is { } branchValue && branch.Source.ConditionalSuccessor is not null) // This branching was conditional
            {
                var constraint = BoolConstraint.From((branch.Source.ConditionKind == ControlFlowConditionKind.WhenTrue) == branch.IsConditionalSuccessor);
                state = state.SetOperationConstraint(branchValue, constraint);
                if (branchValue.TrackedSymbol() is { } symbol)
                {
                    state = state.SetSymbolConstraint(symbol, constraint);
                }
                state = checks.ConditionEvaluated(new(new IOperationWrapperSonar(branchValue), state));
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
            foreach (var preProcessed in checks.PreProcess(new(node.Operation, node.State)))
            {
                var processed = preProcessed.WithState(ProcessOperation(preProcessed));
                foreach (var postProcessed in checks.PostProcess(processed))
                {
                    // When operation doesn't have a parent it is the outer statement. We need to reset operation states:
                    // * We don't need to preserve the inner subexpression intermediate states after the outer statement.
                    // * We don't want ProgramState to contain the path-history data, because we want to avoid exploring the same state twice.
                    // When the operation is a BranchValue, we need to preserve it to evaluate branching. The state will be reset after branching.
                    yield return node.CreateNext(node.Operation.Parent is null && node.Block.BranchValue != node.Operation.Instance ? postProcessed.State.ResetOperations() : postProcessed.State);
                }
            }

            foreach (var successor in ExceptionSuccessors(node, exceptionCandidate.FromOperation(node.Operation)))
            {
                yield return successor;
            }
        }

        private IEnumerable<ExplodedNode> ExceptionSuccessors(ExplodedNode node, ExceptionState exception)
        {
            if (exception is not null && node.Block.EnclosingRegion(ControlFlowRegionKind.Try) is { } tryRegion)
            {
                // We're jumping out of the outer statement => We need to reset operation states.
                var state = node.State.ResetOperations();
                state = node.Block.EnclosingRegion(ControlFlowRegionKind.Catch) is not null
                    ? state.PushException(exception)    // If we're nested inside catch, we need to preserve the exception chain
                    : state.SetException(exception);    // Otherwise we track only the current exception to avoid explosion of states after each statement
                foreach (var catchOrFinally in tryRegion.EnclosingRegion.NestedRegions.Where(x => x.Kind != ControlFlowRegionKind.Try))
                {
                    yield return new(cfg.Blocks[catchOrFinally.FirstBlockOrdinal], state, null);
                }
            }
        }

        private static ExceptionState ThrownException(ExplodedNode node, ControlFlowBranchSemantics semantics) =>
            semantics switch
            {
                ControlFlowBranchSemantics.Throw => ThrowExceptionType(node.Block.BranchValue),
                ControlFlowBranchSemantics.Rethrow => node.State.Exception,
                _ => null
            };

        private static ProgramState ProcessOperation(SymbolicContext context) =>
            context.Operation.Instance.Kind switch
            {
                OperationKindEx.Argument => Invocation.Process(context, IArgumentOperationWrapper.FromOperation(context.Operation.Instance)),
                OperationKindEx.Binary => Binary.Process(context, IBinaryOperationWrapper.FromOperation(context.Operation.Instance)),
                OperationKindEx.Conversion => Conversion.Process(context, IConversionOperationWrapper.FromOperation(context.Operation.Instance)),
                OperationKindEx.FieldReference => References.Process(context, IFieldReferenceOperationWrapper.FromOperation(context.Operation.Instance)),
                OperationKindEx.FlowCapture => FlowCapture.Process(context, IFlowCaptureOperationWrapper.FromOperation(context.Operation.Instance)),
                OperationKindEx.IsPattern => Pattern.Process(context, IIsPatternOperationWrapper.FromOperation(context.Operation.Instance)),
                OperationKindEx.LocalReference => References.Process(context, ILocalReferenceOperationWrapper.FromOperation(context.Operation.Instance)),
                OperationKindEx.ParameterReference => References.Process(context, IParameterReferenceOperationWrapper.FromOperation(context.Operation.Instance)),
                OperationKindEx.SimpleAssignment => SimpleAssignment.Process(context, ISimpleAssignmentOperationWrapper.FromOperation(context.Operation.Instance)),
                _ => context.State
            };

        private static ExceptionState ThrowExceptionType(IOperation operation) =>
            operation.Kind switch
            {
                OperationKindEx.ArrayElementReference => new ExceptionState(operation.Type),
                OperationKindEx.FieldReference => new ExceptionState(operation.Type),
                OperationKindEx.Invocation => new ExceptionState(operation.Type),
                OperationKindEx.LocalReference => new ExceptionState(operation.Type),
                OperationKindEx.ObjectCreation => new ExceptionState(operation.Type),
                OperationKindEx.ParameterReference => new ExceptionState(operation.Type),
                OperationKindEx.PropertyReference => new ExceptionState(operation.Type),
                OperationKindEx.Conversion => ThrowExceptionType(IConversionOperationWrapper.FromOperation(operation).Operand),
                _ => null
            };

        private static bool IsReachable(ExplodedNode node, ControlFlowBranch branch) =>
            node.Block.ConditionKind == ControlFlowConditionKind.None
            || node.State[node.Block.BranchValue] is not { } symbolicValue
            || !symbolicValue.HasConstraint<BoolConstraint>()
            || IsReachable(branch, node.Block.ConditionKind == ControlFlowConditionKind.WhenTrue, symbolicValue.HasConstraint(BoolConstraint.True));

        private static bool IsReachable(ControlFlowBranch branch, bool condition, bool constraint) =>
            branch.IsConditionalSuccessor ? condition == constraint : condition != constraint;
    }
}
