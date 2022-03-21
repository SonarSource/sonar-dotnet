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
using SonarAnalyzer.CFG.Roslyn;
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
        private readonly SymbolicCheck[] checks;
        private readonly Queue<ExplodedNode> queue = new();
        private readonly SymbolicValueCounter symbolicValueCounter = new();
        private readonly HashSet<ExplodedNode> visited = new();

        public RoslynSymbolicExecution(ControlFlowGraph cfg, SymbolicCheck[] checks)
        {
            this.cfg = cfg ?? throw new ArgumentNullException(nameof(cfg));
            if (checks == null || checks.Length == 0)
            {
                throw new ArgumentException("At least one check is expected", nameof(checks));
            }
            this.checks = new[] { new ConstantCheck() }.Concat(checks).ToArray();
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
            NotifyExecutionCompleted();
        }

        private IEnumerable<ExplodedNode> ProcessBranching(ExplodedNode node)
        {
            if (node.Block.Kind == BasicBlockKind.Exit)
            {
                InvokeExitReached(node);
            }
            else if (node.Block.ContainsThrow())
            {
                yield return CreateNode(cfg.ExitBlock, null, null);
            }
            else
            {
                foreach (var successor in node.Block.Successors.Where(x => IsReachable(node, x)))
                {
                    if (successor.Destination is not null)
                    {
                        yield return successor.FinallyRegions.Any() // When exiting finally region(s), redirect to 1st finally instead of the normal destination
                            ? FromFinally(successor, new FinallyPoint(node.FinallyPoint, successor))
                            : CreateNode(successor.Destination, successor, node.FinallyPoint);
                    }
                    else if (successor.Source.EnclosingRegion.Kind == ControlFlowRegionKind.Finally)    // Redirect from finally back to the original place (or outer finally on the same branch)
                    {
                        yield return FromFinally(successor, node.FinallyPoint.CreateNext());
                    }
                }
            }

            ExplodedNode FromFinally(ControlFlowBranch branch, FinallyPoint finallyPoint) =>
                CreateNode(cfg.Blocks[finallyPoint.BlockIndex], branch, finallyPoint.IsFinallyBlock ? finallyPoint : finallyPoint.Previous);

            ExplodedNode CreateNode(BasicBlock block, ControlFlowBranch branch, FinallyPoint finallyPoint) =>
                new(block, ProcessBranch(branch, node.State), finallyPoint);
        }

        private ProgramState ProcessBranch(ControlFlowBranch branch, ProgramState state)
        {
            if (branch is not null)
            {
                foreach (var local in branch.EnteringRegions.SelectMany(x => x.Locals))
                {
                    if (ConstantCheck.ConstraintFromType(local.Type) is { } constraint)
                    {
                        state = state.SetSymbolConstraint(local, symbolicValueCounter, constraint);
                    }
                }
                foreach (var capture in branch.LeavingRegions.SelectMany(x => x.CaptureIds))
                {
                    state = state.RemoveCapture(capture);
                }
            }
            return state.ResetOperations();
        }

        private IEnumerable<ExplodedNode> ProcessOperation(ExplodedNode node)
        {
            foreach (var preProcessed in InvokeChecks(new(symbolicValueCounter, node.Operation, node.State), x => x.PreProcess))
            {
                var processed = EnsureContext(preProcessed, ProcessOperation(preProcessed));
                foreach (var postProcessed in InvokeChecks(processed, x => x.PostProcess))
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

        private void InvokeExitReached(ExplodedNode node)
        {
            var context = new SymbolicContext(symbolicValueCounter, null, node.State);
            foreach (var check in checks)
            {
                check.ExitReached(context);
            }
        }

        private SymbolicContext[] InvokeChecks(SymbolicContext context, Func<SymbolicCheck, Func<SymbolicContext, ProgramState[]>> checkDelegate)
        {
            var contexts = new[] { context };
            foreach (var check in checks)
            {
                var checkMethod = checkDelegate(check);
                contexts = contexts.SelectMany(x => checkMethod(x).Select(newState => EnsureContext(x, newState))).ToArray();
            }
            return contexts;
        }

        private void NotifyExecutionCompleted()
        {
            foreach (var check in checks)
            {
                check.ExecutionCompleted();
            }
        }

        private SymbolicContext EnsureContext(SymbolicContext current, ProgramState newState) =>
            current.State == newState ? current : new SymbolicContext(symbolicValueCounter, current.Operation, newState);

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
