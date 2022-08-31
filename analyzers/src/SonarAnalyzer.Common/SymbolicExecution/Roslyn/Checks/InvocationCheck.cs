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

using Microsoft.CodeAnalysis;
using SonarAnalyzer.SymbolicExecution.Constraints;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.SymbolicExecution.Roslyn.Checks
{
    internal class InvocationCheck : SymbolicCheck
    {
        public override ProgramState[] PreProcess(SymbolicContext context) =>
            context.Operation.Instance.Kind switch
            {
                OperationKindEx.Invocation => PreProcessInvocation(context.State, IInvocationOperationWrapper.FromOperation(context.Operation.Instance)),
                _ => null,
            } is { } newState
                ? newState
                : base.PreProcess(context);

        private ProgramState[] PreProcessInvocation(ProgramState state, IInvocationOperationWrapper invocation)
        {
            if (ArgumentIsNullOrEmpty(invocation) is { } symbol)
            {
                if (state[symbol] is { } symbolicValue && symbolicValue.HasConstraint(ObjectConstraint.Null))
                {
                    // IsNullOrEmpty will always return "true". For the opposite case, we can not tell for sure
                    return new[] { SetOperationConstraint(BoolConstraint.True) };
                }

                return new[]
                {
                    SetOperationConstraint(BoolConstraint.True).SetSymbolConstraint(symbol, ObjectConstraint.Null),
                    SetOperationConstraint(BoolConstraint.True).SetSymbolConstraint(symbol, ObjectConstraint.NotNull),
                    SetOperationConstraint(BoolConstraint.False).SetSymbolConstraint(symbol, ObjectConstraint.NotNull),
                };
            }

            return null;

            ProgramState SetOperationConstraint(BoolConstraint boolConstraint) =>
                state.SetOperationConstraint(invocation.WrappedOperation, boolConstraint);

        }

        private static ISymbol ArgumentIsNullOrEmpty(IInvocationOperationWrapper invocation) =>
            invocation is
            {
                TargetMethod:
                {
                    IsStatic: true,
                    ContainingType.SpecialType: SpecialType.System_String,
                    Name: nameof(string.IsNullOrEmpty) or nameof(string.IsNullOrWhiteSpace),
                    Parameters: { Length: 1 } parameters
                },
                Arguments: { Length: 1 } arguments,
            }
            && parameters[0] is { Name: "value", Type.SpecialType: SpecialType.System_String }
            && arguments[0].TrackedSymbol() is { } symbol
                ? symbol
                : null;
    }
}
