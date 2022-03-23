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
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.SymbolicExecution.Constraints;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.SymbolicExecution.Roslyn.RuleChecks
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
        private static readonly string[] IsLockHeldProperties =
        {
            "IsReadLockHeld",
            "IsReaderLockHeld",
            "IsWriteLockHeld",
            "IsWriterLockHeld",
            "IsUpgradeableReadLockHeld",
        };

        protected abstract ISafeSyntaxWalker CreateSyntaxWalker(LockAcquireReleaseCollector collector);

        public override void ExitReached(SymbolicContext context) =>
            exitHeldSymbols.AddRange(context.State.SymbolsWith(LockConstraint.Held));

        public override void ExecutionCompleted()
        {
            foreach (var unreleasedSymbol in exitHeldSymbols.Intersect(releasedSymbols).Where(x => lastSymbolLock.ContainsKey(x)))
            {
                ReportIssue(lastSymbolLock[unreleasedSymbol]);
            }
        }

        public override bool ShouldExecute()
        {
            var collector = new LockAcquireReleaseCollector();
            var walker = CreateSyntaxWalker(collector);
            foreach (var child in NodeContext.Node.ChildNodes())
            {
                walker.SafeVisit(child);
            }
            return collector.LockAcquiredAndReleased;
        }

        public override ProgramState ConditionEvaluated(SymbolicContext context)
        {
            if (context.Operation.Instance.AsPropertyReference() is { } property
                && IsLockHeldProperties.Contains(property.Property.Name)
                && property.Property.ContainingType.IsAny(KnownType.System_Threading_ReaderWriterLock, KnownType.System_Threading_ReaderWriterLockSlim)
                && property.Instance.TrackedSymbol() is { } lockSymbol)
            {
                return context.State[context.Operation].HasConstraint(BoolConstraint.True)  // Is it a branch with the Lock held?
                    ? AddLock(context, lockSymbol)
                    : RemoveLock(context, lockSymbol);
            }
            else
            {
                return context.State;
            }
        }

        protected override ProgramState PostProcessSimple(SymbolicContext context)
        {
            if (context.Operation.Instance.AsObjectCreation() is { } objectCreation)
            {
                if (objectCreation.Type.Is(KnownType.System_Threading_Mutex)
                    && context.Operation.Parent.AsAssignment() is { } assignment
                    && objectCreation.Arguments.Length > 0
                    && objectCreation.Arguments.First().ToArgument().Value is { } firstArgument
                    && context.State[firstArgument] is { } firstArgumentValue
                    && firstArgumentValue.HasConstraint(BoolConstraint.True)
                    && assignment.Target.TrackedSymbol() is { } symbol)
                {
                    lastSymbolLock[symbol] = new IOperationWrapperSonar(objectCreation.WrappedOperation);
                    return AddLock(context, objectCreation.WrappedOperation).Preserve(symbol);
                }
            }
            else if (context.Operation.Instance.AsInvocation() is { } invocation)
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
                         || invocation.TargetMethod.Is(KnownType.System_Threading_WaitHandle, "WaitOne")
                         || invocation.TargetMethod.IsAny(KnownType.System_Threading_SpinLock, "Enter", "TryEnter"))
                {
                    return ProcessInvocationInstanceAcquireLock(context, invocation);
                }
                else if (invocation.TargetMethod.IsAny(KnownType.System_Threading_ReaderWriterLock, "ReleaseLock", "ReleaseReaderLock", "ReleaseWriterLock")
                         || invocation.TargetMethod.IsAny(KnownType.System_Threading_ReaderWriterLockSlim, "ExitReadLock", "ExitWriteLock", "ExitUpgradeableReadLock")
                         || invocation.TargetMethod.Is(KnownType.System_Threading_Mutex, "ReleaseMutex")
                         || invocation.TargetMethod.Is(KnownType.System_Threading_SpinLock, "Exit"))
                {
                    return ProcessInvocationInstanceReleaseLock(context, invocation);
                }
            }
            return context.State;
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
            return context.SetSymbolConstraint(symbol, LockConstraint.Held).Preserve(symbol);
        }

        private ProgramState RemoveLock(SymbolicContext context, ISymbol symbol)
        {
            if (symbol == null)
            {
                return context.State;
            }

            releasedSymbols.Add(symbol);
            return context.SetSymbolConstraint(symbol, LockConstraint.Released).Preserve(symbol);
        }

        // This method should be removed once the engine has support for `True/False` boolean constraints.
        private static ProgramState AddLock(SymbolicContext context, IOperation operation) =>
            context.State.SetOperationConstraint(operation, context.SymbolicValueCounter, LockConstraint.Held);

        private static ISymbol FirstArgumentSymbol(IInvocationOperationWrapper invocation) =>
            IArgumentOperationWrapper.FromOperation(invocation.Arguments.First()).Value.TrackedSymbol();

        protected sealed class LockAcquireReleaseCollector
        {
            private static readonly string LockType = "Mutex"; // For some APIs ctor can directly acquire the lock (e.g. Mutex).

            private static readonly HashSet<string> LockMethods = new(ReaderWriterLockSlimLockMethods)
            {
                "AcquireReaderLock",
                "AcquireWriterLock",
                "Enter",
                "TryEnter",
                "WaitOne"
            };

            private static readonly HashSet<string> ReleaseMethods = new()
            {
                "Exit",
                "ExitReadLock",
                "ExitUpgradeableReadLock",
                "ExitWriteLock",
                "ReleaseLock",
                "ReleaseMutex",
                "ReleaseReaderLock",
                "ReleaseWriterLock"
            };

            private bool lockAcquired;
            private bool lockReleased;

            public bool LockAcquiredAndReleased =>
                lockAcquired && lockReleased;

            public void RegisterIdentifier(string name)
            {
                lockAcquired = lockAcquired || name == LockType || LockMethods.Contains(name) || IsLockHeldProperties.Contains(name);
                lockReleased = lockReleased || ReleaseMethods.Contains(name);
            }
        }
    }
}
