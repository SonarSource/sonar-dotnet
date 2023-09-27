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

using SonarAnalyzer.SymbolicExecution.Constraints;

namespace SonarAnalyzer.SymbolicExecution.Roslyn.RuleChecks;

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
        foreach (var child in Node.ChildNodes())
        {
            walker.SafeVisit(child);
        }
        return collector.LockAcquiredAndReleased;
    }

    public override ProgramStates PostProcess(SymbolicContext context)
    {
        if (context.Operation.Instance.Kind == OperationKindEx.Invocation)
        {
            if (FindRefParam(context) is { } refParamContext)
            {
                return new(
                  refParamContext.SetRefConstraint(BoolConstraint.True, AddLock(refParamContext.SymbolicContext, refParamContext.LockSymbol)),
                  refParamContext.SetRefConstraint(BoolConstraint.False, refParamContext.SymbolicContext.State));
            }
            else if (FindLockSymbolWithConditionalReturnValue(context) is { } lockSymbol)
            {
                return new(
                    AddLock(context, lockSymbol).SetOperationConstraint(context.Operation, BoolConstraint.True),
                    context.SetOperationConstraint(BoolConstraint.False));
            }
        }
        else if (context.Operation.Instance.AsObjectCreation() is { } objectCreation
            && objectCreation.Type.Is(KnownType.System_Threading_Mutex)
            && context.Operation.Parent.AsAssignment() is { } assignment
            && assignment.Target.TrackedSymbol(context.State) is { } symbol
            && objectCreation.ArgumentValue("initiallyOwned") is { } initiallyOwned
            && context.State[initiallyOwned] is { } initiallyOwnedValue
            && initiallyOwnedValue.HasConstraint(BoolConstraint.True))
        {
            lastSymbolLock[symbol] = objectCreation.ToSonar();
            return objectCreation.ArgumentValue("createdNew") is { } createdNew
                && createdNew.TrackedSymbol(context.State) is { } trackedCreatedNew
                ? new(
                    AddLock(context, objectCreation.WrappedOperation).Preserve(symbol).SetSymbolConstraint(trackedCreatedNew, BoolConstraint.True),
                    context.SetSymbolConstraint(trackedCreatedNew, BoolConstraint.False))
                : new(AddLock(context, objectCreation.WrappedOperation).Preserve(symbol));
        }

        return base.PostProcess(context);
    }

    public override ProgramState ConditionEvaluated(SymbolicContext context)
    {
        if (context.Operation.Instance.AsPropertyReference() is { } property
            && IsLockHeldProperties.Contains(property.Property.Name)
            && ((IMemberReferenceOperationWrapper)property).IsOnReaderWriterLockOrSlim())
        {
            return ProcessCondition(property.Instance.TrackedSymbol(context.State));
        }
        else if (context.Operation.Instance.AsInvocation() is { } invocation && invocation.IsMonitorIsEntered())    // Same condition also needs to be in ExceptionCandidate
        {
            return ProcessCondition(ArgumentSymbol(context.State, invocation, 0));
        }
        else
        {
            return context.State;
        }

        ProgramState ProcessCondition(ISymbol lockSymbol)
        {
            if (lockSymbol is null)
            {
                return context.State;
            }
            else
            {
                return context.State[context.Operation].HasConstraint(BoolConstraint.True)  // Is it a branch with the Lock held?
                    ? AddLock(context, lockSymbol)
                    : RemoveLock(context, lockSymbol);
            }
        }
    }

    protected override ProgramState PostProcessSimple(SymbolicContext context)
    {
        if (context.Operation.Instance.AsInvocation() is { } invocation)
        {
            // ToDo: we ignore the number of parameters for now.
            if (invocation.TargetMethod.IsAny(KnownType.System_Threading_Monitor, "Enter", "TryEnter"))
            {
                return ProcessMonitorEnter(context, invocation);
            }
            else if (invocation.IsMonitorExit())    // Same condition also needs to be in ExceptionCandidate
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
            else if (invocation.IsLockRelease())    // Same condition also needs to be in ExceptionCandidate
            {
                return ProcessInvocationInstanceReleaseLock(context, invocation);
            }
        }
        return context.State;
    }

    private ProgramState ProcessMonitorEnter(SymbolicContext context, IInvocationOperationWrapper invocation) =>
        AddLock(context, ArgumentSymbol(context.State, invocation, 0));

    private ProgramState ProcessMonitorExit(SymbolicContext context, IInvocationOperationWrapper invocation) =>
        RemoveLock(context, ArgumentSymbol(context.State, invocation, 0));

    private ProgramState ProcessInvocationInstanceAcquireLock(SymbolicContext context, IInvocationOperationWrapper invocation) =>
        AddLock(context, invocation.Instance.TrackedSymbol(context.State));

    private ProgramState ProcessInvocationInstanceReleaseLock(SymbolicContext context, IInvocationOperationWrapper invocation) =>
        RemoveLock(context, invocation.Instance.TrackedSymbol(context.State));

    private ProgramState AddLock(SymbolicContext context, ISymbol symbol)
    {
        if (symbol == null)
        {
            return context.State;
        }

        lastSymbolLock[symbol] = context.Operation;
        return context.SetSymbolConstraint(symbol, LockConstraint.Held).SetSymbolConstraint(symbol, ObjectConstraint.NotNull).Preserve(symbol);
    }

    private ProgramState RemoveLock(SymbolicContext context, ISymbol symbol)
    {
        if (!context.State.HasConstraint(symbol, LockConstraint.Held))
        {
            return context.State;
        }

        releasedSymbols.Add(symbol);
        return context.SetSymbolConstraint(symbol, LockConstraint.Released).Preserve(symbol);
    }

    // This method should be removed once the engine has support for `True/False` boolean constraints.
    private static ProgramState AddLock(SymbolicContext context, IOperation operation) =>
        context.State.SetOperationConstraint(operation, LockConstraint.Held);

    private static ISymbol ArgumentSymbol(ProgramState state, IInvocationOperationWrapper invocation, int parameterIndex) =>
        invocation.TargetMethod.Parameters[parameterIndex].Name is var parameterName
        && invocation.Arguments[parameterIndex].ToArgument() is var argument
        && argument.Parameter.Name == parameterName
            ? argument.Value.TrackedSymbol(state)
            : invocation.Arguments.SingleOrDefault(x => x.ToArgument().Parameter.Name == parameterName)?.ToArgument().Value.TrackedSymbol(state);

    private static ISymbol FindLockSymbolWithConditionalReturnValue(SymbolicContext context)
    {
        if (context.Operation.Instance.AsInvocation().Value is var invocation
            && invocation.TargetMethod.ReturnType.Is(KnownType.System_Boolean))
        {
            if (invocation.TargetMethod.IsAny(KnownType.System_Threading_Monitor, "TryEnter"))
            {
                return ArgumentSymbol(context.State, invocation, 0);
            }
            else if (invocation.TargetMethod.IsAny(KnownType.System_Threading_WaitHandle, "WaitOne")
                     || invocation.TargetMethod.IsAny(KnownType.System_Threading_ReaderWriterLockSlim, "TryEnterReadLock", "TryEnterUpgradeableReadLock", "TryEnterWriteLock"))
            {
                return invocation.Instance.TrackedSymbol(context.State);
            }
        }
        return null;
    }

    private static RefParamContext FindRefParam(SymbolicContext context) =>
        BoolRefParamFromArgument(context, KnownType.System_Threading_Monitor, "Enter", "TryEnter")
        ?? BoolRefParamFromInstance(context, KnownType.System_Threading_SpinLock, "Enter", "TryEnter");

    private static RefParamContext BoolRefParamFromArgument(SymbolicContext context, KnownType type, params string[] methodNames) =>
        context.Operation.Instance.AsInvocation().Value is var invocation
        && InvocationBoolRefSymbol(context.State, invocation, type, methodNames) is { } refParameter
        && ArgumentSymbol(context.State, invocation, 0) is { } lockObject
            ? new RefParamContext(context, lockObject, refParameter)
            : null;

    private static RefParamContext BoolRefParamFromInstance(SymbolicContext context, KnownType type, params string[] methodNames) =>
        context.Operation.Instance.AsInvocation().Value is var invocation
        && InvocationBoolRefSymbol(context.State, invocation, type, methodNames) is { } refParameter
        && invocation.Instance.TrackedSymbol(context.State) is { } lockObject
            ? new RefParamContext(context, lockObject, refParameter)
            : null;

    private static ISymbol InvocationBoolRefSymbol(ProgramState state, IInvocationOperationWrapper invocation, KnownType type, params string[] methodNames) =>
        invocation.TargetMethod.IsAny(type, methodNames)
        && invocation.TargetMethod.Parameters.AsEnumerable().IndexOf(x => x.RefKind == RefKind.Ref && x.IsType(KnownType.System_Boolean)) is var index
        && index >= 0
            ? ArgumentSymbol(state, invocation, index)
            : null;

    protected sealed class LockAcquireReleaseCollector
    {
        private const string LockType = "Mutex"; // For some APIs ctor can directly acquire the lock (e.g. Mutex).

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
            lockAcquired = lockAcquired || name == LockType || LockMethods.Contains(name) || IsLockHeldProperties.Contains(name) || name == "IsEntered";
            lockReleased = lockReleased || ReleaseMethods.Contains(name);
        }
    }

    private sealed record RefParamContext(SymbolicContext SymbolicContext, ISymbol LockSymbol, ISymbol RefParamSymbol)
    {
        public ProgramState SetRefConstraint(SymbolicConstraint constraint, ProgramState state) =>
            state.SetSymbolConstraint(RefParamSymbol, constraint);
    }
}
