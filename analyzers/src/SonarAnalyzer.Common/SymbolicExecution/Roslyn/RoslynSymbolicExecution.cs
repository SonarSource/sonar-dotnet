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
            queue.Enqueue(new ExplodedNode(cfg.EntryBlock, ProgramState.Empty, null));
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
                InvokeChecks(new SymbolicContext(symbolicValueCounter, null, node.State), x => x.ExitReached);
            }
            else if (node.Block.ContainsThrow())
            {
                yield return new ExplodedNode(cfg.ExitBlock, node.State, null);
            }
            else
            {
                // ToDo: This is a temporary simplification until we support condition-based and condition-building decisions https://github.com/SonarSource/sonar-dotnet/issues/5308
                foreach (var successor in node.Block.Successors)
                {
                    if (successor.Destination is not null)
                    {
                        yield return successor.FinallyRegions.Any() // When exiting finally region(s), redirect to 1st finally instead of the normal destination
                            ? FromFinally(new FinallyPoint(cfg, successor))
                            : new ExplodedNode(successor.Destination, node.State, node.FinallyPoint);
                    }
                    else if (successor.Source.EnclosingRegion.Kind == ControlFlowRegionKind.Finally)    // Redirect from finally back to the original place (or outer finally on the same branch)
                    {
                        yield return FromFinally(node.FinallyPoint.CreateNext());
                    }
                }
            }

            ExplodedNode FromFinally(FinallyPoint finallyPoint) =>
                new ExplodedNode(finallyPoint.Block, node.State, finallyPoint.IsFinallyBlock ? finallyPoint : null);
        }

        private IEnumerable<ExplodedNode> ProcessOperation(ExplodedNode node)
        {
            var context = new SymbolicContext(symbolicValueCounter, node.Operation, node.State);
            context = InvokeChecks(context, x => x.PreProcess);
            if (context != null)
            {
                context = EnsureContext(context, ProcessOperation(context));
                context = InvokeChecks(context, x => x.PostProcess);
                if (context != null)
                {
                    // When operation doesn't have a parent it is the outer statement. We need to reset operation states:
                    // * We don't need to preserve the inner subexpression intermediate states after the outer statement.
                    // * We don't want ProgramState to contain the path-history data, because we want to avoid exploring the same state twice.
                    yield return node.CreateNext(node.Operation.Parent is null ? context.State.ResetOperations() : context.State);
                }
            }
        }

        private static ProgramState ProcessOperation(SymbolicContext context) =>
            context.Operation.Instance.Kind switch
            {
                OperationKindEx.Conversion => Conversion.Process(context, IConversionOperationWrapper.FromOperation(context.Operation.Instance)),
                OperationKindEx.FieldReference => References.Process(context, IFieldReferenceOperationWrapper.FromOperation(context.Operation.Instance)),
                OperationKindEx.LocalReference => References.Process(context, ILocalReferenceOperationWrapper.FromOperation(context.Operation.Instance)),
                OperationKindEx.ParameterReference => References.Process(context, IParameterReferenceOperationWrapper.FromOperation(context.Operation.Instance)),
                OperationKindEx.SimpleAssignment => SimpleAssignment.Process(context, ISimpleAssignmentOperationWrapper.FromOperation(context.Operation.Instance)),
                _ => context.State
            };

        private SymbolicContext InvokeChecks(SymbolicContext context, Func<SymbolicCheck, Func<SymbolicContext, ProgramState>> checkDelegate)
        {
            foreach (var check in checks)
            {
                var checkMethod = checkDelegate(check);
                var newState = checkMethod(context);
                if (newState == null)
                {
                    return null;
                }
                context = EnsureContext(context, newState);
            }
            return context;
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
    }
}
