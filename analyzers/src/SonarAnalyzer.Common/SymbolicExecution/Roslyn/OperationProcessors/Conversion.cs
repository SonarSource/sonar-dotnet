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

    protected override ProgramState[] Process(SymbolicContext context, IConversionOperationWrapper conversion) =>
        IsBuildIn(conversion.OperatorMethod)
            ? LearnFromOperand(context, conversion).ToArray()
            : new[] { context.State };

    private static bool IsBuildIn(ISymbol symbol) =>
        symbol is null || symbol.ContainingType.IsAny(KnownType.PointerTypes);

    private static IEnumerable<ProgramState> LearnFromOperand(SymbolicContext context, IConversionOperationWrapper conversion)
    {
        var value = context.State[conversion.Operand] ?? SymbolicValue.Empty;
        if (IsUncertainTryCast(conversion))
        {
            yield return context.SetOperationValue(SymbolicValue.Null);
            if (value.HasConstraint(ObjectConstraint.Null) is false)
            {
                var notNull = context.SetOperationValue(value.WithConstraint(ObjectConstraint.NotNull));
                yield return conversion.Operand.TrackedSymbol(context.State) is { } symbol
                    ? notNull.SetSymbolConstraint(symbol, ObjectConstraint.NotNull) : notNull;
            }
        }
        else
        {
            yield return value == SymbolicValue.Empty ? context.State : context.SetOperationValue(value);
        }
    }

    private static bool IsUncertainTryCast(IConversionOperationWrapper conversion) =>
        conversion.IsTryCast
        && !conversion.Operand.Type.DerivesOrImplements(conversion.Type)
        && !(conversion.Operand.Type.IsNonNullableValueType() && conversion.Type.IsNullableValueType());
}
