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
        private readonly SymbolicValueCounter symbolicValueCounter = new();
        private readonly HashSet<ExplodedNode> visited = new();
        private readonly RoslynLiveVariableAnalysis lva;

        public RoslynSymbolicExecution(ControlFlowGraph cfg, SymbolicCheck[] checks, ISymbol declaration)
        {
            this.cfg = cfg ?? throw new ArgumentNullException(nameof(cfg));
            if (checks == null || checks.Length == 0)
            {
                throw new ArgumentException("At least one check is expected", nameof(checks));
            }
            _ = declaration ?? throw new ArgumentNullException(nameof(declaration));
            this.checks = new(new[] { new ConstantCheck() }.Concat(checks).ToArray());
            lva = new RoslynLiveVariableAnalysis(cfg, declaration);
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
                    var successors = current.Operation == null ? ProcessBranching(current) : ProcessOperation(current);
                    foreach (var node in successors)
                    {
                        queue.Enqueue(node);
                    }
                }
            }
            checks.ExecutionCompleted();
        }

        private IEnumerable<ExplodedNode> ProcessBranching(ExplodedNode node)
        {
            if (node.Block.Kind == BasicBlockKind.Exit)
            {
                checks.ExitReached(new(symbolicValueCounter, null, node.State));
            }
            else if (node.Block.ContainsThrow())
            {
                node = new ExplodedNode(cfg.ExitBlock, CleanUnusedState(node.State, node.Block), null);
                yield return new(cfg.ExitBlock, node.State, null);
            }
            else
            {
                var reachableSuccessors = node.Block.Successors.Where(x => IsReachable(node, x)).ToList();
                node = new ExplodedNode(node.Block, CleanUnusedState(node.State, node.Block), node.FinallyPoint);
                foreach (var successor in reachableSuccessors)
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
            else if (branch.Source.EnclosingRegion.Kind == ControlFlowRegionKind.Finally)    // Redirect from finally back to the original place (or outer finally on the same branch)
            {
                return FromFinally(node.FinallyPoint.CreateNext());
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
                    state = state.SetSymbolConstraint(local, symbolicValueCounter, constraint);
                }
            }
            if (branch.Source.BranchValue is { } branchValue && branch.Source.ConditionalSuccessor is not null) // This branching was conditional
            {
                var constraint = BoolConstraint.From((branch.Source.ConditionKind == ControlFlowConditionKind.WhenTrue) == branch.IsConditionalSuccessor);
                state = state.SetOperationConstraint(branchValue, symbolicValueCounter, constraint);
                if (branchValue.TrackedSymbol() is { } symbol)
                {
                    state = state.SetSymbolConstraint(symbol, symbolicValueCounter, constraint);
                }
                state = checks.ConditionEvaluated(new(symbolicValueCounter, new IOperationWrapperSonar(branchValue), state));
                if (state is null)
                {
                    return null;
                }
            }
            foreach (var capture in branch.LeavingRegions.SelectMany(x => x.CaptureIds))
            {
                state = state.RemoveCapture(capture);
            }
            return state.ResetOperations();
        }

        private IEnumerable<ExplodedNode> ProcessOperation(ExplodedNode node)
        {
            foreach (var preProcessed in checks.PreProcess(new(symbolicValueCounter, node.Operation, node.State)))
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
        }

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

        private ProgramState CleanUnusedState(ProgramState programState, BasicBlock block)
        {
            var liveVariables = lva.LiveOut(block);
            return programState.RemoveSymbols(x => (x is ILocalSymbol or IParameterSymbol { RefKind:RefKind.None }) && !liveVariables.Contains(x));
        }

        private static bool IsReachable(ExplodedNode node, ControlFlowBranch branch) =>
            node.Block.ConditionKind != ControlFlowConditionKind.None
            && node.State[node.Block.BranchValue] is { } sv
            && sv.HasConstraint<BoolConstraint>()
                ? IsReachable(branch, node.Block.ConditionKind == ControlFlowConditionKind.WhenTrue, sv.HasConstraint(BoolConstraint.True))
                : true;    // Unconditional or we don't know the value and need to explore both paths

        private static bool IsReachable(ControlFlowBranch branch, bool condition, bool constraint) =>
            branch.IsConditionalSuccessor ? condition == constraint : condition != constraint;
    }
}
