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

namespace SonarAnalyzer.SymbolicExecution.Roslyn.Checks;

internal class ConstantCheck : SymbolicCheck
{
    protected override ProgramState PreProcessSimple(SymbolicContext context)
    {
        var state = context.State;
        var operation = context.Operation.Instance;
        if (operation.ConstantValue.HasValue && ConstraintFromConstantValue(context.Operation) is { } value)
        {
            state = context.SetOperationValue(value);
            if (operation.TrackedSymbol(state) is { } symbol && state[symbol] is null)
            {
                state = state.SetSymbolValue(symbol, value);
            }
        }
        return state;
    }

    public static SymbolicConstraint ConstraintFromType(ITypeSymbol type)
    {
        if (type.Is(KnownType.System_Boolean))
        {
            return BoolConstraint.False;
        }
        else if (type.IsReferenceType)
        {
            return ObjectConstraint.Null;
        }
        else if (type.IsAny(KnownType.IntegralNumbersIncludingNative))
        {
            return NumberConstraint.From(0);
        }
        else
        {
            return null;
        }
    }

    private static SymbolicValue ConstraintFromConstantValue(IOperationWrapperSonar operation) =>
        operation.Instance.ConstantValue.Value switch
        {
            // Update DefaultValue when adding new types
            true => SymbolicValue.True,
            false => SymbolicValue.False,
            null when CanBeNull(operation) => SymbolicValue.Null,
            string => SymbolicValue.NotNull,
            _ when NumberConstraint.From(operation.Instance.ConstantValue.Value) is { } number => SymbolicValue.NotNull.WithConstraint(number),
            _ => null
        };

    private static ITypeSymbol ConvertedType(IOperation operation) =>
        // Some version of Roslyn can send null here with "return default;". It's not reproducible by UTs, as we have "Type" set in that case.
        operation?.Kind == OperationKindEx.Conversion ? operation.ToConversion().Type : null;

    private static bool CanBeNull(IOperationWrapperSonar operation)
    {
        var type = operation.Instance.Type ?? ConvertedType(operation.Parent);
        return type is null || type.CanBeNull();
    }
}
