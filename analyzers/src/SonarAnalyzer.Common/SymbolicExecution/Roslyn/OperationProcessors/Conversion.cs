/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

namespace SonarAnalyzer.SymbolicExecution.Roslyn.OperationProcessors;

internal sealed class Conversion : MultiProcessor<IConversionOperationWrapper>
{
    protected override IConversionOperationWrapper Convert(IOperation operation) =>
        operation.ToConversion();

    protected override ProgramState[] Process(SymbolicContext context, IConversionOperationWrapper conversion)
    {
        if (IsBuildIn(conversion.OperatorMethod))
        {
            var operandValue = context.State[conversion.Operand];
            return !conversion.IsTryCast
                || conversion.Operand.Type.DerivesOrImplements(conversion.Type)
                || (conversion.Operand.Type.IsNonNullableValueType() && conversion.Type.IsNullableValueType())
                    ? ForRegularConversionState(context, operandValue).ToArray()
                    : UncertainTryCastStates(context, operandValue);
        }
        else
        {
            return context.State.ToArray();
        }
    }

    private static bool IsBuildIn(ISymbol symbol) =>
        symbol?.ContainingType.IsAny(KnownType.PointerTypes) is not false;

    private static ProgramState ForRegularConversionState(SymbolicContext context, SymbolicValue operandValue) =>
        operandValue == null ? context.State : context.SetOperationValue(operandValue);

    private static ProgramState[] UncertainTryCastStates(SymbolicContext context, SymbolicValue operandValue) =>
        operandValue?.HasConstraint(ObjectConstraint.Null) is true
            ? context.SetOperationValue(SymbolicValue.Null).ToArray()
            : new[]
            {
                context.SetOperationValue(SymbolicValue.Null),
                context.SetOperationValue((operandValue ?? SymbolicValue.Empty).WithConstraint(ObjectConstraint.NotNull))
            };
}
