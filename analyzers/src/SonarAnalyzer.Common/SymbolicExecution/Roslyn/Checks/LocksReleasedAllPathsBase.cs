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
using SonarAnalyzer.Helpers;
using SonarAnalyzer.SymbolicExecution.Constraints;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.SymbolicExecution.Roslyn.Checks
{
    public abstract class LocksReleasedAllPathsBase : SymbolicRuleCheck
    {
        internal const string DiagnosticId = "S2222";
        protected const string MessageFormat = "Unlock this lock along all executions paths of this method.";
        private readonly HashSet<ISymbol> previouslyReleasedSymbols = new();
        private readonly HashSet<ISymbol> exitHeldSymbols = new();
        private readonly Dictionary<ISymbol, IOperationWrapperSonar> symbolOperationMap = new();

        protected abstract DiagnosticDescriptor Rule { get; }

        // ToDo: Implement early bail-out if there's no interesting descendant node in context.Node to avoid useless SE runs
        // ToDo: Update this to be disabled by default.
        public override bool ShouldExecute() =>
            true;

        public override ProgramState PostProcess(SymbolicContext context)
        {
            var currentState = context.State;

            if (context.Operation.Instance.Kind == OperationKindEx.Invocation)
            {
                var invocationWrapper = IInvocationOperationWrapper.FromOperation(context.Operation.Instance);
                // ToDo: we ignore the number of parameters for now.
                if (invocationWrapper.TargetMethod.Is(KnownType.System_Threading_Monitor, "Enter")
                    || invocationWrapper.TargetMethod.Is(KnownType.System_Threading_Monitor, "TryEnter"))
                {
                    currentState = ProcessMonitorEnter(invocationWrapper, currentState, context);
                }
                else if (invocationWrapper.TargetMethod.Is(KnownType.System_Threading_Monitor, "Exit"))
                {
                    currentState = ProcessMonitorExit(invocationWrapper, currentState, context);
                }
            }

            return currentState;
        }

        public override ProgramState ExitReached(SymbolicContext context)
        {
            exitHeldSymbols.AddRange(context.State.SymbolsWith(LockConstraint.Held));
            return base.ExitReached(context);
        }

        public override void ExecutionCompleted()
        {
            var unreleasedSymbols = exitHeldSymbols.Intersect(previouslyReleasedSymbols);
            foreach (var symbol in unreleasedSymbols)
            {
                var location = symbolOperationMap[symbol].Instance.Syntax.GetLocation();
                NodeContext.ReportIssue(Diagnostic.Create(Rule, location));
            }
        }

        private ProgramState ProcessMonitorEnter(IInvocationOperationWrapper invocationWrapper, ProgramState currentState, SymbolicContext context)
        {
            var lockObjectSymbol = GetParameterValueSymbol(invocationWrapper);
            if (lockObjectSymbol == null)
            {
                return currentState;
            }

            if (currentState[lockObjectSymbol] == null)
            {
                currentState = currentState.SetSymbolValue(lockObjectSymbol, context.CreateSymbolicValue());
            }

            // Should we handle the cases when the mutex is already held or released?
            currentState[lockObjectSymbol].SetConstraint(LockConstraint.Held);
            symbolOperationMap[lockObjectSymbol] = context.Operation;
            return currentState;
        }

        private ProgramState ProcessMonitorExit(IInvocationOperationWrapper invocationWrapper, ProgramState currentState, SymbolicContext context)
        {
            var lockObjectSymbol = GetParameterValueSymbol(invocationWrapper);
            if (lockObjectSymbol == null)
            {
                return currentState;
            }

            if (currentState[lockObjectSymbol] == null)
            {
                // In this case the mutex has been released without being held.
                currentState = currentState.SetSymbolValue(lockObjectSymbol, context.CreateSymbolicValue());
            }

            currentState[lockObjectSymbol].SetConstraint(LockConstraint.Released);
            previouslyReleasedSymbols.Add(lockObjectSymbol);
            return currentState;
        }

        private static ISymbol GetParameterValueSymbol(IInvocationOperationWrapper invocationWrapper) =>
            IArgumentOperationWrapper.FromOperation(invocationWrapper.Arguments.First()).Value.TrackedSymbol();
    }
}
