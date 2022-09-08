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

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.SymbolicExecution.Constraints;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.SymbolicExecution.Roslyn.OperationProcessors
{
    internal static class Invocation
    {
        public static ProgramState Process(SymbolicContext context, IInvocationOperationWrapper invocation)
        {
            var state = context.State;
            if (!invocation.TargetMethod.IsStatic             // Also applies to C# extensions
                && !invocation.TargetMethod.IsExtensionMethod // VB extensions in modules are not marked as static
                && invocation.Instance.TrackedSymbol() is { } symbol)
            {
                state = state.SetSymbolConstraint(symbol, ObjectConstraint.NotNull);
            }
            if (invocation.IsReceiverThis())
            {
                state = state.ResetFieldConstraints();
            }
            return state;
        }

        public static ProgramState Process(SymbolicContext context, IArgumentOperationWrapper argument) =>
            ProcessArgument(context.State, argument) ?? context.State;

        private static ProgramState ProcessArgument(ProgramState state, IArgumentOperationWrapper argument)
        {
            if (argument.Parameter is null)
            {
                return null; // __arglist is not assigned to a parameter
            }
            if (argument is { Parameter.RefKind: RefKind.Out or RefKind.Ref } && argument.Value.TrackedSymbol() is { } symbol)
            {
                state = state.SetSymbolValue(symbol, null); // Forget state for "out" or "ref" arguments
            }
            if (argument.Parameter.GetAttributes() is { Length: > 0 } attributes)
            {
                state = ProcessArgumentAttributes(state, argument, attributes);
            }
            return state;
        }

        private static ProgramState ProcessArgumentAttributes(ProgramState state, IArgumentOperationWrapper argument, ImmutableArray<AttributeData> attributes) =>
            attributes.Any(IsValidatedNotNullAttribute) && argument.Value.TrackedSymbol() is { } symbol
                ? state.SetSymbolConstraint(symbol, ObjectConstraint.NotNull)
                : state;

        // Same as [NotNull] https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/attributes/nullable-analysis#postconditions-maybenull-and-notnull
        private static bool IsValidatedNotNullAttribute(AttributeData attribute) =>
            attribute.AttributeClass?.Name == "ValidatedNotNullAttribute";
    }
}
