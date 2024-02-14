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

internal sealed class Binary : BinaryBase<IBinaryOperationWrapper>
{
    protected override IBinaryOperationWrapper Convert(IOperation operation) =>
        IBinaryOperationWrapper.FromOperation(operation);

    protected override SymbolicConstraint BoolConstraintFromOperation(ProgramState state, IBinaryOperationWrapper operation) =>
        BoolConstraintFromBinaryOperation(operation.OperatorKind, state[operation.LeftOperand], state[operation.RightOperand]);

    protected override ProgramState LearnBranchingConstraint(ProgramState state, IBinaryOperationWrapper operation, bool falseBranch)
    {
        if (operation.OperatorKind.IsAnyEquality())
        {
            state = LearnBranchingEqualityConstraint(state, operation.LeftOperand, operation.RightOperand, operation.OperatorKind, falseBranch);
        }
        else if (operation.OperatorKind.IsAnyRelational())
        {
            state = LearnBranchingRelationalObjectConstraint(state, operation.LeftOperand, operation.RightOperand, falseBranch) ?? state;
            state = LearnBranchingNumberConstraint(state, operation.LeftOperand, operation.RightOperand, operation.OperatorKind, falseBranch);
            state = LearnBranchingCollectionConstraint(state, operation.LeftOperand, operation.RightOperand, operation.OperatorKind, falseBranch);
        }
        return state;
    }

    protected override ProgramState PreProcess(ProgramState state, IBinaryOperationWrapper operation, bool isInLoop)
    {
        if (state[operation.LeftOperand]?.Constraint<NumberConstraint>() is { } leftNumber
            && state[operation.RightOperand]?.Constraint<NumberConstraint>() is { } rightNumber
            && ArithmeticCalculator.Calculate(operation.OperatorKind, leftNumber, rightNumber, isInLoop) is { } constraint)
        {
            state = state.SetOperationConstraint(operation, constraint);
        }
        if (operation.Type.Is(KnownType.System_String) && operation.OperatorKind is BinaryOperatorKind.Add or BinaryOperatorKind.Concatenate)
        {
            state = state.SetOperationConstraint(operation, ObjectConstraint.NotNull);
        }
        return state;
    }
}