/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.SymbolicExecution.Constraints;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.SymbolicExecution.Roslyn.Checks
{
    public abstract class LocksReleasedAllPathsBase : SymbolicRuleCheck
    {
        internal const string DiagnosticId = "S2222";
        protected const string MessageFormat = "Unlock this lock along all executions paths of this method.";

        private readonly HashSet<ISymbol> releasedSymbols = new();
        private readonly HashSet<ISymbol> exitHeldSymbols = new();
        private readonly Dictionary<ISymbol, IOperationWrapperSonar> symbolOperationMap = new();

        // ToDo: Implement early bail-out if there's no interesting descendant node in context.Node to avoid useless SE runs
        public override bool ShouldExecute() =>
            NodeContext.Node.DescendantNodes().OfType<IdentifierNameSyntax>().Any(x => x.Identifier.Text == "Exit");

        public override ProgramState PostProcess(SymbolicContext context)
        {
            if (context.Operation.Instance.AsInvocation() is { } invocation)
            {
                // ToDo: we ignore the number of parameters for now.
                if (invocation.TargetMethod.Is(KnownType.System_Threading_Monitor, "Enter")
                    || invocation.TargetMethod.Is(KnownType.System_Threading_Monitor, "TryEnter"))
                {
                    return ProcessMonitorEnter(invocation, context);
                }
                else if (invocation.TargetMethod.Is(KnownType.System_Threading_Monitor, "Exit"))
                {
                    return ProcessMonitorExit(invocation, context);
                }
            }
            return context.State;
        }

        public override ProgramState ExitReached(SymbolicContext context)
        {
            exitHeldSymbols.AddRange(context.State.SymbolsWith(LockConstraint.Held));
            return base.ExitReached(context);
        }

        public override void ExecutionCompleted()
        {
            foreach (var unreleasedSymbol in exitHeldSymbols.Intersect(releasedSymbols))
            {
                ReportIssue(symbolOperationMap[unreleasedSymbol]);
            }
        }

        private ProgramState ProcessMonitorEnter(IInvocationOperationWrapper invocationWrapper, SymbolicContext context)
        {
            var lockObjectSymbol = FirstArgumentSymbol(invocationWrapper);
            if (lockObjectSymbol == null)
            {
                return context.State;
            }

            var state = context.State;
            if (state[lockObjectSymbol] == null)
            {
                state = state.SetSymbolValue(lockObjectSymbol, context.CreateSymbolicValue());
            }

            state[lockObjectSymbol].SetConstraint(LockConstraint.Held);
            symbolOperationMap[lockObjectSymbol] = context.Operation;
            return state;
        }

        private ProgramState ProcessMonitorExit(IInvocationOperationWrapper invocationWrapper, SymbolicContext context)
        {
            var lockObjectSymbol = FirstArgumentSymbol(invocationWrapper);
            if (lockObjectSymbol == null)
            {
                return context.State;
            }

            var state = context.State;
            if (state[lockObjectSymbol] == null)
            {
                // In this case the mutex has been released without being held.
                state = state.SetSymbolValue(lockObjectSymbol, context.CreateSymbolicValue());
            }

            state[lockObjectSymbol].SetConstraint(LockConstraint.Released);
            releasedSymbols.Add(lockObjectSymbol);
            return state;
        }

        private static ISymbol FirstArgumentSymbol(IInvocationOperationWrapper invocationWrapper) =>
            IArgumentOperationWrapper.FromOperation(invocationWrapper.Arguments.First()).Value.TrackedSymbol();
    }
}
