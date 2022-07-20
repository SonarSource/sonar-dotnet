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

using SonarAnalyzer.SymbolicExecution.Constraints;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.SymbolicExecution.Roslyn.OperationProcessors
{
    internal static class Invocation
    {
        public static ProgramState Process(SymbolicContext context, IInvocationOperationWrapper invocation) =>
            !invocation.TargetMethod.IsStatic               // Also applies to C# extensions
            && !invocation.TargetMethod.IsExtensionMethod   // VB extensions in modules are not marked as static
            && invocation.Instance.TrackedSymbol() is { } symbol
                ? context.SetSymbolConstraint(symbol, ObjectConstraint.NotNull)
                : context.State;

        public static ProgramState Process(SymbolicContext context, IArgumentOperationWrapper argument) =>
            argument.Parameter is not null      // __arglist is not assigned to a parameter
            && argument.Parameter.RefKind != Microsoft.CodeAnalysis.RefKind.None
            && argument.Value.TrackedSymbol() is { } symbol
                ? context.State.SetSymbolValue(symbol, null)
                : context.State;
    }
}
