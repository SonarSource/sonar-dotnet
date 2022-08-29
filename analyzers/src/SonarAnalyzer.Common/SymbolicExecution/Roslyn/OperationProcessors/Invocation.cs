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
using Microsoft.CodeAnalysis;
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
            ProcessArgument(context.State, argument) ?? context.State;

        private static ProgramState ProcessArgument(ProgramState state, IArgumentOperationWrapper argument) =>
            argument switch
            {
                { Parameter: null } => null, // __arglist is not assigned to a parameter
                { Parameter.RefKind: not RefKind.None, Value: { } value } when value.TrackedSymbol() is { } symbol =>
                    // The argument is passed by some kind of reference, so we need to forget all we knew about it.
                    state.SetSymbolValue(symbol, null),
                { Parameter: { } parameter } when parameter.GetAttributes() is { Length: > 0 } attributes =>
                    // Learn from parameter nullable annotations
                    ProcessArgumentAttributes(state, argument, attributes),
                _ => null,
            };

        private static ProgramState ProcessArgumentAttributes(ProgramState state, IArgumentOperationWrapper argument, ImmutableArray<AttributeData> attributes)
        {
            foreach (var attribute in attributes)
            {
                if (IsValidatedNotNullAttribute(attribute) && argument.Value.TrackedSymbol() is { } symbol)
                {
                    return state.SetSymbolConstraint(symbol, ObjectConstraint.NotNull);
                }
            }
            return null;
        }

        // Copy of SonarAnalyzer.CSharp\SymbolicExecution\Sonar\InvocationVisitor.cs
        // Same as [NotNull] https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/attributes/nullable-analysis#postconditions-maybenull-and-notnull
        private static bool IsValidatedNotNullAttribute(AttributeData attribute) =>
            "ValidatedNotNullAttribute".Equals(attribute.AttributeClass?.Name);
    }
}
