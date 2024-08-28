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

namespace SonarAnalyzer.SymbolicExecution.Roslyn.OperationProcessors;

internal sealed class Unary : SimpleProcessor<IUnaryOperationWrapper>
{
    protected override IUnaryOperationWrapper Convert(IOperation operation) =>
        IUnaryOperationWrapper.FromOperation(operation);

    protected override ProgramState Process(SymbolicContext context, IUnaryOperationWrapper unary) =>
        unary.OperatorKind == UnaryOperatorKind.Not && context.State[unary.Operand] is { } operandValue
            ? ProcessNot(context.State, unary, operandValue)
            : context.State;

    private static ProgramState ProcessNot(ProgramState state, IUnaryOperationWrapper unary, SymbolicValue operandValue)
    {
        if (operandValue.Constraint<BoolConstraint>() is { } boolConstraint)
        {
            return state.SetOperationConstraint(unary.WrappedOperation, boolConstraint.Opposite);
        }
        else if (operandValue.HasConstraint(ObjectConstraint.Null))
        {
            return state.SetOperationConstraint(unary.WrappedOperation, ObjectConstraint.Null);
        }
        else
        {
            return state;
        }
    }
}
