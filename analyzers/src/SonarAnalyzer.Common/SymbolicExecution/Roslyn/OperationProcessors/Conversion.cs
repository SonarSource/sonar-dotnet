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
        if (conversion.Type.IsValueType || conversion.Type.IsUnconstraintGeneric() || conversion.Operand.Type.IsUnconstraintGeneric())
        {
            return new[] { state };
        }
        var operandConstraint = operandSymbolValue?.Constraint<ObjectConstraint>();
        var operandSymbol = conversion.Operand.TrackedSymbol();
        var targetCanBeNull = conversion.Operand.Type is { IsReferenceType: true };
        return operandConstraint switch
        {
            { } when operandConstraint == ObjectConstraint.Null => new[] { ProcessNull(state, conversion, operandSymbol) },
            { } when operandConstraint == ObjectConstraint.NotNull => new[] { ProcessNotNull(state, conversion, operandSymbol) },
            //_ when targetCanBeNull && conversion.Type is { } targetType && conversion.Operand.Type is { } sourceType && sourceType.DerivesOrImplements(targetType) => new[] { state },
            _ when !targetCanBeNull => new[] { state },
            _ => new[] { ProcessNotNull(state, conversion, operandSymbol), ProcessNull(state, conversion, operandSymbol) },
        };
    }

    private static ProgramState ProcessNotNull(ProgramState state, IConversionOperationWrapper conversion, ISymbol operandSymbol)
    {
        var notNull = state.SetOperationConstraint(conversion.WrappedOperation, ObjectConstraint.NotNull);
        if (operandSymbol is { } && conversion.Operand.Type?.IsReferenceType is true)
        {
            notNull = notNull.SetSymbolConstraint(operandSymbol, ObjectConstraint.NotNull);
        }

        return notNull;
    }

    private static ProgramState ProcessNull(ProgramState state, IConversionOperationWrapper conversion, ISymbol operandSymbol)
    {
        var @null = state.SetOperationConstraint(conversion.WrappedOperation, ObjectConstraint.Null);
        if (operandSymbol is { })
        {
            if (conversion.IsTryCast)
            {
                var isUpCast = conversion.Operand.Type.DerivesOrImplements(conversion.Type);
                if (isUpCast)
                {
                    @null = @null.SetSymbolConstraint(operandSymbol, ObjectConstraint.Null);
                }
            }
            else
            {
                @null = @null.SetSymbolConstraint(operandSymbol, ObjectConstraint.Null);
            }
        }

        return @null;
    }
}
