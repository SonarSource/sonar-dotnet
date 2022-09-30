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

using SonarAnalyzer.Helpers;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.SymbolicExecution.Roslyn
{
    internal static class IInvocationOperationExtensions
    {
        public static bool IsMonitorExit(this IInvocationOperationWrapper invocation) =>
            invocation.TargetMethod.Is(KnownType.System_Threading_Monitor, "Exit");

        public static bool IsMonitorIsEntered(this IInvocationOperationWrapper invocation) =>
            invocation.TargetMethod.Is(KnownType.System_Threading_Monitor, "IsEntered");

        public static bool IsLockRelease(this IInvocationOperationWrapper invocation) =>
            invocation.TargetMethod.IsAny(KnownType.System_Threading_ReaderWriterLock, "ReleaseLock", "ReleaseReaderLock", "ReleaseWriterLock")
            || invocation.TargetMethod.IsAny(KnownType.System_Threading_ReaderWriterLockSlim, "ExitReadLock", "ExitWriteLock", "ExitUpgradeableReadLock")
            || invocation.TargetMethod.Is(KnownType.System_Threading_Mutex, "ReleaseMutex")
            || invocation.TargetMethod.Is(KnownType.System_Threading_SpinLock, "Exit");

        /// <summary>
        /// Returns <see langword="true"/>, if the method is an instance method, or an extension method, where the argument passed to the receiver parameter is <see langword="this"/>.
        /// </summary>
        public static bool HasThisReceiver(this IInvocationOperationWrapper invocation, ProgramState state) =>
            state.ResolveCapture(invocation.Instance.UnwrapConversion()) is { Kind: OperationKindEx.InstanceReference }
            || (invocation is { TargetMethod.IsExtensionMethod: true, Arguments: { Length: > 0 } arguments }
                && arguments[0] is { Kind: OperationKindEx.Argument } thisArgument
                && IArgumentOperationWrapper.FromOperation(state.ResolveCapture(thisArgument.UnwrapConversion())).Value.UnwrapConversion() is { Kind: OperationKindEx.InstanceReference });
    }
}
