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
using SonarAnalyzer.Helpers;
using SonarAnalyzer.SymbolicExecution.Constraints;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.SymbolicExecution.Roslyn.OperationProcessors;

internal sealed class Conversion : MultiProcessor<IConversionOperationWrapper>
{
    protected override IConversionOperationWrapper Convert(IOperation operation) =>
        IConversionOperationWrapper.FromOperation(operation);

    protected override ProgramState[] Process(SymbolicContext context, IConversionOperationWrapper conversion)
    {
        var operandSymbolValue = context.State[conversion.Operand];
        var state = operandSymbolValue is { }
            ? context.State.SetOperationValue(context.Operation, operandSymbolValue)
            : context.State;
        return Process(state, conversion, operandSymbolValue);
    }

    private static ProgramState[] Process(ProgramState state, IConversionOperationWrapper conversion, SymbolicValue operandSymbolValue)
    {
        var operandConstraint = operandSymbolValue?.Constraint<ObjectConstraint>();
        var operandSymbol = conversion.Operand.TrackedSymbol();
        var targetCanBeNull = conversion.Operand.Type is { IsReferenceType: true };
        return true switch
        {
            _ when IsBoxingConversion(conversion) => ProcessBoxing(state, conversion),
            _ when IsUnboxingConversion(conversion) => ProcessUnboxing(state, operandSymbol),
            _ => new[] { state },
        };
    }

    private static bool IsUnboxingConversion(IConversionOperationWrapper conversion) =>
        conversion is { Type.IsValueType: true, Operand.Type.IsReferenceType: true };

    private static ProgramState[] ProcessUnboxing(ProgramState state, ISymbol operandSymbol) =>
        operandSymbol == null
            ? new[] { state }
            : new[] { state.SetSymbolConstraint(operandSymbol, ObjectConstraint.NotNull) };

    private static bool IsBoxingConversion(IConversionOperationWrapper conversion) =>
        conversion is { Type.IsReferenceType: true, Operand.Type.IsValueType: true };

    private static ProgramState[] ProcessBoxing(ProgramState state, IConversionOperationWrapper conversion) =>
        conversion.Operand.Type.IsNullable()
        ? new[] { state }
        : new[] { state.SetOperationConstraint(conversion.WrappedOperation, ObjectConstraint.NotNull) };

}
