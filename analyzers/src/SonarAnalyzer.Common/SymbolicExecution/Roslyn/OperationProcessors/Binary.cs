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

internal sealed class Binary : BranchingProcessor<IBinaryOperationWrapper>
{
    protected override IBinaryOperationWrapper Convert(IOperation operation) =>
        IBinaryOperationWrapper.FromOperation(operation);

    protected override SymbolicConstraint BoolConstraintFromOperation(ProgramState state, IBinaryOperationWrapper operation) =>
        BinaryConstraint(operation.OperatorKind, state[operation.LeftOperand], state[operation.RightOperand]);

    protected override ProgramState LearnBranchingConstraint(ProgramState state, IBinaryOperationWrapper operation, bool falseBranch)
    {
        if (operation.OperatorKind.IsAnyEquality())
        {
            state = LearnBranchingEqualityConstraint<ObjectConstraint>(state, operation, falseBranch) ?? state;
            state = LearnBranchingEqualityConstraint<BoolConstraint>(state, operation, falseBranch) ?? state;
        }
        else if (operation.OperatorKind.IsAnyRelational())
        {
            state = LearnBranchingRelationalConstraint(state, operation, falseBranch) ?? state;
        }
        return state;
    }

    private static ProgramState LearnBranchingEqualityConstraint<T>(ProgramState state, IBinaryOperationWrapper binary, bool falseBranch)
        where T : SymbolicConstraint
    {
        var useOpposite = falseBranch ^ binary.OperatorKind.IsNotEquals();
        // We can take the 1st constraint and "testedSymbol" because they are exclusive. Symbols with T constraint will be recognized as the constraining side.
        if (FirstBinaryOperandConstraint<T>(state, binary) is { } constraint
            && FirstBinaryOperandSymbolWithoutConstraint<T>(state, binary) is { } testedSymbol
            && !(useOpposite && constraint is BoolConstraint && testedSymbol.GetSymbolType().IsNullableBoolean()))  // Don't learn False for "nullableBool != true", because it could also be <null>.
        {
            constraint = constraint.ApplyOpposite(useOpposite);     // Beware that opposite of ObjectConstraint.NotNull doesn't exist and returns <null>
            return constraint is null ? null : state.SetSymbolConstraint(testedSymbol, constraint);
        }
        else
        {
            return null;
        }
    }

    // We can take the 1st constraint and "testedSymbol" because they are exclusive. Symbols with NotNull constraint will be recognized as the constraining side.
    // We only learn in the true branch because not being >, >=, <, <= than a non-empty nullable means either being null or non-null with non-matching value.
    private static ProgramState LearnBranchingRelationalConstraint(ProgramState state, IBinaryOperationWrapper binary, bool falseBranch) =>
        !falseBranch
        && FirstBinaryOperandConstraint<ObjectConstraint>(state, binary) == ObjectConstraint.NotNull
        && FirstBinaryOperandSymbolWithoutConstraint<ObjectConstraint>(state, binary) is { } testedSymbol
        && testedSymbol.GetSymbolType().IsNullableValueType()
            ? state.SetSymbolConstraint(testedSymbol, ObjectConstraint.NotNull)
            : null;

    private static SymbolicConstraint FirstBinaryOperandConstraint<T>(ProgramState state, IBinaryOperationWrapper binary) where T : SymbolicConstraint =>
        OperandConstraint<T>(state, binary.LeftOperand) ?? OperandConstraint<T>(state, binary.RightOperand);

    private static ISymbol FirstBinaryOperandSymbolWithoutConstraint<T>(ProgramState state, IBinaryOperationWrapper binary) where T : SymbolicConstraint =>
        OperandSymbolWithoutConstraint<T>(state, binary.LeftOperand) ?? OperandSymbolWithoutConstraint<T>(state, binary.RightOperand);

    private static ISymbol OperandSymbolWithoutConstraint<T>(ProgramState state, IOperation candidate) where T : SymbolicConstraint =>
        candidate.TrackedSymbol() is { } symbol && (state[symbol] is null || !state[symbol].HasConstraint<T>()) ? symbol : null;

    private static SymbolicConstraint OperandConstraint<T>(ProgramState state, IOperation candidate) where T : SymbolicConstraint =>
        state[candidate] is { } value && value.HasConstraint<T>() ? value.Constraint<T>() : null;

    private static SymbolicConstraint BinaryConstraint(BinaryOperatorKind kind, SymbolicValue left, SymbolicValue right)
    {
        var leftBool = left?.Constraint<BoolConstraint>();
        var rightBool = right?.Constraint<BoolConstraint>();
        if (leftBool is null ^ rightBool is null)
        {
            return kind switch
            {
                BinaryOperatorKind.Or or BinaryOperatorKind.ConditionalOr when (leftBool ?? rightBool) == BoolConstraint.True => BoolConstraint.True,
                BinaryOperatorKind.And or BinaryOperatorKind.ConditionalAnd when (leftBool ?? rightBool) == BoolConstraint.False => BoolConstraint.False,
                _ => null
            };
        }
        else if (leftBool is not null && rightBool is not null)
        {
            return BinaryBoolConstraint(kind, leftBool == BoolConstraint.True, rightBool == BoolConstraint.True);
        }
        else if (left?.HasConstraint<ObjectConstraint>() is true && right?.HasConstraint<ObjectConstraint>() is true)
        {
            return BinaryNullConstraint(kind, left.HasConstraint(ObjectConstraint.Null), right.HasConstraint(ObjectConstraint.Null));
        }
        else if (left?.HasConstraint(ObjectConstraint.Null) is true || right?.HasConstraint(ObjectConstraint.Null) is true)
        {
            return kind.IsAnyRelational() ? BoolConstraint.False : null;
        }
        else
        {
            return null;
        }
    }

    private static SymbolicConstraint BinaryBoolConstraint(BinaryOperatorKind kind, bool left, bool right) =>
        kind switch
        {
            BinaryOperatorKind.Equals or BinaryOperatorKind.ObjectValueEquals => BoolConstraint.From(left == right),
            BinaryOperatorKind.NotEquals or BinaryOperatorKind.ObjectValueNotEquals => BoolConstraint.From(left != right),
            BinaryOperatorKind.And or BinaryOperatorKind.ConditionalAnd => BoolConstraint.From(left && right),
            BinaryOperatorKind.Or or BinaryOperatorKind.ConditionalOr => BoolConstraint.From(left || right),
            BinaryOperatorKind.ExclusiveOr => BoolConstraint.From(left ^ right),
            _ => null
        };

    private static SymbolicConstraint BinaryNullConstraint(BinaryOperatorKind kind, bool isNullLeft, bool isNullRight) =>
        isNullLeft || isNullRight
            ? kind switch
            {
                BinaryOperatorKind.Equals or BinaryOperatorKind.ObjectValueEquals => BoolConstraint.From(isNullLeft && isNullRight),
                BinaryOperatorKind.NotEquals or BinaryOperatorKind.ObjectValueNotEquals => BoolConstraint.From(isNullLeft != isNullRight),
                _ when kind.IsAnyRelational() => BoolConstraint.False,
                _ => null
            }
            : null;
}
