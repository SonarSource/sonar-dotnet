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
        private readonly Dictionary<ISymbol, IOperationWrapperSonar> lastSymbolLock = new();

        private static readonly string[] ReaderWriterLockSlimLockMethods =
        {
            "EnterReadLock",
            "EnterUpgradeableReadLock",
            "EnterWriteLock",
            "TryEnterReadLock",
            "TryEnterUpgradeableReadLock",
            "TryEnterWriteLock"
        };

        // ToDo: Implement early bail-out if there's no interesting descendant node in context.Node to avoid useless SE runs
        public override bool ShouldExecute() =>
            NodeContext.Node.DescendantNodes().OfType<IdentifierNameSyntax>().Any(x => x.Identifier.Text.Contains("Exit")
                                                                                       || x.Identifier.Text.Contains("ReleaseMutex")
                                                                                       || x.Identifier.Text.Contains("ReleaseReaderLock"));

        public override ProgramState PostProcess(SymbolicContext context)
        {
            if (context.Operation.Instance.AsInvocation() is { } invocation)
            {
                // ToDo: we ignore the number of parameters for now.
                if (invocation.TargetMethod.IsAny(KnownType.System_Threading_Monitor, "Enter", "TryEnter"))
                {
                    return ProcessMonitorEnter(context, invocation);
                }
                else if (invocation.TargetMethod.Is(KnownType.System_Threading_Monitor, "Exit"))
                {
                    return ProcessMonitorExit(context, invocation);
                }
                else if (invocation.TargetMethod.IsAny(KnownType.System_Threading_ReaderWriterLock, "AcquireReaderLock", "AcquireWriterLock")
                         || invocation.TargetMethod.IsAny(KnownType.System_Threading_ReaderWriterLockSlim, ReaderWriterLockSlimLockMethods)
                         || invocation.TargetMethod.Is(KnownType.System_Threading_WaitHandle, "WaitOne"))
                {
                    return ProcessInvocationInstanceAcquireLock(context, invocation);
                }
                else if (invocation.TargetMethod.IsAny(KnownType.System_Threading_ReaderWriterLock, "ReleaseLock", "ReleaseReaderLock", "ReleaseWriterLock")
                         || invocation.TargetMethod.IsAny(KnownType.System_Threading_ReaderWriterLockSlim, "ExitReadLock", "ExitWriteLock", "ExitUpgradeableReadLock")
                         || invocation.TargetMethod.Is(KnownType.System_Threading_Mutex, "ReleaseMutex"))
                {
                    return ProcessInvocationInstanceReleaseLock(context, invocation);
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
                ReportIssue(lastSymbolLock[unreleasedSymbol]);
            }
        }

        private ProgramState ProcessMonitorEnter(SymbolicContext context, IInvocationOperationWrapper invocation) =>
            AddLock(context, FirstArgumentSymbol(invocation));

        private ProgramState ProcessMonitorExit(SymbolicContext context, IInvocationOperationWrapper invocation) =>
            RemoveLock(context, FirstArgumentSymbol(invocation));

        private ProgramState ProcessInvocationInstanceAcquireLock(SymbolicContext context, IInvocationOperationWrapper invocation) =>
            AddLock(context, invocation.Instance.TrackedSymbol());

        private ProgramState ProcessInvocationInstanceReleaseLock(SymbolicContext context, IInvocationOperationWrapper invocation) =>
            RemoveLock(context, invocation.Instance.TrackedSymbol());

        private ProgramState AddLock(SymbolicContext context, ISymbol symbol)
        {
            if (symbol == null)
            {
                return context.State;
            }

            lastSymbolLock[symbol] = context.Operation;
            return context.SetSymbolConstraint(symbol, LockConstraint.Held);
        }

        private ProgramState RemoveLock(SymbolicContext context, ISymbol symbol)
        {
            if (symbol == null)
            {
                return context.State;
            }

            releasedSymbols.Add(symbol);
            return context.SetSymbolConstraint(symbol, LockConstraint.Released);
        }

        private static ISymbol FirstArgumentSymbol(IInvocationOperationWrapper invocation) =>
            IArgumentOperationWrapper.FromOperation(invocation.Arguments.First()).Value.TrackedSymbol();
    }
}
