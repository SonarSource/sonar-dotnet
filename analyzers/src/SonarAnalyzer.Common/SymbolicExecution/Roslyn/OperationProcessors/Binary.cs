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

    protected override SymbolicConstraint BoolConstraintFromOperation(ProgramState state, IBinaryOperationWrapper operation, int visitCount) =>
        BinaryConstraint(operation.OperatorKind, state[operation.LeftOperand], state[operation.RightOperand], visitCount);

    protected override ProgramState LearnBranchingConstraint(ProgramState state, IBinaryOperationWrapper operation, bool falseBranch)
    {
        if (operation.OperatorKind.IsAnyEquality())
        {
            state = LearnBranchingEqualityConstraint<ObjectConstraint>(state, operation, falseBranch) ?? state;
            state = LearnBranchingEqualityConstraint<BoolConstraint>(state, operation, falseBranch) ?? state;
            state = LearnBranchingNumberConstraint(state, operation, falseBranch) ?? state;
        }
        else if (operation.OperatorKind.IsAnyRelational())
        {
            state = LearnBranchingRelationalObjectConstraint(state, operation, falseBranch) ?? state;
            state = LearnBranchingNumberConstraint(state, operation, falseBranch) ?? state;
        }
        return state;
    }

    protected override ProgramState PreProcess(ProgramState state, IBinaryOperationWrapper operation)
    {
        if (state[operation.LeftOperand]?.Constraint<NumberConstraint>() is { } leftNumber
            && state[operation.RightOperand]?.Constraint<NumberConstraint>() is { } rightNumber
            && Calculate(operation.OperatorKind, leftNumber, rightNumber) is { } constraint)
        {
            state = state.SetOperationConstraint(operation, constraint);
        }
        return state;
    }

    private static NumberConstraint Calculate(BinaryOperatorKind kind, NumberConstraint left, NumberConstraint right) => kind switch
    {
        BinaryOperatorKind.Add => NumberConstraint.From(left.Min + right.Min, left.Max + right.Max),
        BinaryOperatorKind.Subtract => NumberConstraint.From(left.Min - right.Max, left.Max - right.Min),
        BinaryOperatorKind.Multiply => CalculateMultiply(left, right),
        _ => null
    };

    private static NumberConstraint CalculateMultiply(NumberConstraint left, NumberConstraint right)
    {
        var products = new[] { left.Min * right.Min, left.Min * right.Max, left.Max * right.Min, left.Max * right.Max };
        var min = (left.Min is null && right.CanBePositive)
            || (right.Min is null && left.CanBePositive)
            || (left.Max is null && right.CanBeNegative)
            || (right.Max is null && left.CanBeNegative)
            ? null
            : products.Min();
        var max = (left.Min is null && right.CanBeNegative)
            || (right.Min is null && left.CanBeNegative)
            || (left.Max is null && right.CanBePositive)
            || (right.Max is null && left.CanBePositive)
            ? null
            : products.Max();
        return NumberConstraint.From(min, max);
    }

    private static ProgramState LearnBranchingEqualityConstraint<T>(ProgramState state, IBinaryOperationWrapper binary, bool falseBranch)
        where T : SymbolicConstraint
    {
        var useOpposite = falseBranch ^ binary.OperatorKind.IsNotEquals();
        // We can take left or right constraint and "testedSymbol" because they are exclusive. Symbols with T constraint will be recognized as the constraining side.
        if (BinaryOperandConstraint<T>(state, binary) is { } constraint
            && BinaryOperandSymbolWithoutConstraint<T>(state, binary) is { } testedSymbol
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

    // We can take the left or right constraint and "testedSymbol" because they are exclusive. Symbols with NotNull constraint will be recognized as the constraining side.
    // We only learn in the true branch because not being >, >=, <, <= than a non-empty nullable means either being null or non-null with non-matching value.
    private static ProgramState LearnBranchingRelationalObjectConstraint(ProgramState state, IBinaryOperationWrapper binary, bool falseBranch) =>
        !falseBranch
        && BinaryOperandConstraint<ObjectConstraint>(state, binary) == ObjectConstraint.NotNull
        && BinaryOperandSymbolWithoutConstraint<ObjectConstraint>(state, binary) is { } testedSymbol
        && testedSymbol.GetSymbolType().IsNullableValueType()
            ? state.SetSymbolConstraint(testedSymbol, ObjectConstraint.NotNull)
            : null;

    private static ProgramState LearnBranchingNumberConstraint(ProgramState state, IBinaryOperationWrapper binary, bool falseBranch)
    {
        var kind = falseBranch ? Opposite(binary.OperatorKind) : binary.OperatorKind;
        var leftNumber = state[binary.LeftOperand]?.Constraint<NumberConstraint>();
        var rightNumber = state[binary.RightOperand]?.Constraint<NumberConstraint>();
        if (rightNumber is not null && binary.LeftOperand.TrackedSymbol() is { } leftSymbol)
        {
            return LearnBranching(leftSymbol, leftNumber, kind, rightNumber);
        }
        else if (leftNumber is not null && binary.RightOperand.TrackedSymbol() is { } rightSymbol)
        {
            return LearnBranching(rightSymbol, rightNumber, Flip(kind), leftNumber);
        }
        else
        {
            return null;
        }

        ProgramState LearnBranching(ISymbol symbol, NumberConstraint existingNumber, BinaryOperatorKind kind, NumberConstraint comparedNumber) =>
            !(falseBranch && symbol.GetSymbolType().IsNullableValueType())  // Don't learn opposite for "nullable > 0", because it could also be <null>.
            && RelationalNumberConstraint(falseBranch ? null : existingNumber, kind, comparedNumber) is { } newConstraint
                ? state.SetSymbolConstraint(symbol, newConstraint)
                : null;

        static NumberConstraint RelationalNumberConstraint(NumberConstraint existingNumber, BinaryOperatorKind kind, NumberConstraint comparedNumber) =>
            kind switch
            {
                BinaryOperatorKind.Equals => comparedNumber,
                BinaryOperatorKind.NotEquals when comparedNumber.IsSingleValue && comparedNumber.Min == existingNumber?.Min => NumberConstraint.From(existingNumber.Min + 1, existingNumber.Max),
                BinaryOperatorKind.NotEquals when comparedNumber.IsSingleValue && comparedNumber.Min == existingNumber?.Max => NumberConstraint.From(existingNumber.Min, existingNumber.Max - 1),
                BinaryOperatorKind.GreaterThan when comparedNumber.Min.HasValue => NumberConstraint.From(comparedNumber.Min + 1, existingNumber?.Max),
                BinaryOperatorKind.GreaterThanOrEqual when comparedNumber.Min.HasValue => NumberConstraint.From(comparedNumber.Min, existingNumber?.Max),
                BinaryOperatorKind.LessThan when comparedNumber.Max.HasValue => NumberConstraint.From(existingNumber?.Min, comparedNumber.Max - 1),
                BinaryOperatorKind.LessThanOrEqual when comparedNumber.Max.HasValue => NumberConstraint.From(existingNumber?.Min, comparedNumber.Max),
                _ => null
            };

        static BinaryOperatorKind Flip(BinaryOperatorKind kind) =>
            kind switch
            {
                BinaryOperatorKind.Equals => BinaryOperatorKind.Equals,
                BinaryOperatorKind.NotEquals => BinaryOperatorKind.NotEquals,
                BinaryOperatorKind.GreaterThan => BinaryOperatorKind.LessThan,
                BinaryOperatorKind.GreaterThanOrEqual => BinaryOperatorKind.LessThanOrEqual,
                BinaryOperatorKind.LessThan => BinaryOperatorKind.GreaterThan,
                BinaryOperatorKind.LessThanOrEqual => BinaryOperatorKind.GreaterThanOrEqual,
                _ => BinaryOperatorKind.None    // Unreachable under normal conditions, VB ObjectValueEquals would need comparison with a number
            };

        static BinaryOperatorKind Opposite(BinaryOperatorKind kind) =>
            kind switch
            {
                BinaryOperatorKind.Equals => BinaryOperatorKind.NotEquals,
                BinaryOperatorKind.NotEquals => BinaryOperatorKind.Equals,
                BinaryOperatorKind.GreaterThan => BinaryOperatorKind.LessThanOrEqual,
                BinaryOperatorKind.GreaterThanOrEqual => BinaryOperatorKind.LessThan,
                BinaryOperatorKind.LessThan => BinaryOperatorKind.GreaterThanOrEqual,
                BinaryOperatorKind.LessThanOrEqual => BinaryOperatorKind.GreaterThan,
                _ => BinaryOperatorKind.None    // We don't care about ObjectValueEquals
            };
    }

    private static SymbolicConstraint BinaryOperandConstraint<T>(ProgramState state, IBinaryOperationWrapper binary) where T : SymbolicConstraint =>
        state[binary.LeftOperand]?.Constraint<T>() ?? state[binary.RightOperand]?.Constraint<T>();

    private static ISymbol BinaryOperandSymbolWithoutConstraint<T>(ProgramState state, IBinaryOperationWrapper binary) where T : SymbolicConstraint =>
        OperandSymbolWithoutConstraint<T>(state, binary.LeftOperand) ?? OperandSymbolWithoutConstraint<T>(state, binary.RightOperand);

    private static ISymbol OperandSymbolWithoutConstraint<T>(ProgramState state, IOperation candidate) where T : SymbolicConstraint =>
        candidate.TrackedSymbol() is { } symbol
        && (state[symbol] is null || !state[symbol].HasConstraint<T>())
            ? symbol
            : null;

    private static SymbolicConstraint BinaryConstraint(BinaryOperatorKind kind, SymbolicValue left, SymbolicValue right, int visitCount)
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
        else if (left?.Constraint<NumberConstraint>() is { } leftNumber
            && right?.Constraint<NumberConstraint>() is { } rightNumber
            && visitCount == 1)    // Fixed loops: 1st visit decides on value, 2nd visit learns range instead to be able to exit the loop
        {
            return BinaryNumberConstraint(kind, leftNumber, rightNumber);
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

    private static SymbolicConstraint BinaryNumberConstraint(BinaryOperatorKind kind, NumberConstraint left, NumberConstraint right) =>
        kind switch
        {
            BinaryOperatorKind.Equals when left.IsSingleValue && right.IsSingleValue => BoolConstraint.From(left.Equals(right)),
            BinaryOperatorKind.Equals when left.IsSingleValue && !right.CanContain(left.Min.Value) => BoolConstraint.False,
            BinaryOperatorKind.Equals when right.IsSingleValue && !left.CanContain(right.Min.Value) => BoolConstraint.False,
            BinaryOperatorKind.NotEquals when left.IsSingleValue && right.IsSingleValue => BoolConstraint.From(!left.Equals(right)),
            BinaryOperatorKind.GreaterThan when left.Min > right.Max => BoolConstraint.True,
            BinaryOperatorKind.GreaterThan when left.Max <= right.Min => BoolConstraint.False,
            BinaryOperatorKind.GreaterThanOrEqual when left.Min >= right.Max => BoolConstraint.True,
            BinaryOperatorKind.GreaterThanOrEqual when left.Max < right.Min => BoolConstraint.False,
            BinaryOperatorKind.LessThan when left.Max < right.Min => BoolConstraint.True,
            BinaryOperatorKind.LessThan when left.Min >= right.Max => BoolConstraint.False,
            BinaryOperatorKind.LessThanOrEqual when left.Max <= right.Min => BoolConstraint.True,
            BinaryOperatorKind.LessThanOrEqual when left.Min > right.Max => BoolConstraint.False,
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
