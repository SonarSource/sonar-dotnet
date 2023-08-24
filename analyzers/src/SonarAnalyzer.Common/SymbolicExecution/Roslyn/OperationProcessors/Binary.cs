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

using System.Numerics;
using SonarAnalyzer.SymbolicExecution.Constraints;

namespace SonarAnalyzer.SymbolicExecution.Roslyn.OperationProcessors;

internal sealed partial class Binary : BranchingProcessor<IBinaryOperationWrapper>
{
    protected override IBinaryOperationWrapper Convert(IOperation operation) =>
        IBinaryOperationWrapper.FromOperation(operation);

    protected override SymbolicConstraint BoolConstraintFromOperation(ProgramState state, IBinaryOperationWrapper operation, bool isLoopCondition, int visitCount) =>
        BinaryConstraint(operation.OperatorKind, state[operation.LeftOperand], state[operation.RightOperand], isLoopCondition, visitCount);

    protected override ProgramState LearnBranchingConstraint(ProgramState state, IBinaryOperationWrapper operation, bool isLoopCondition, int visitCount, bool falseBranch)
    {
        if (operation.OperatorKind.IsAnyEquality())
        {
            state = LearnBranchingEqualityConstraint<ObjectConstraint>(state, operation, falseBranch) ?? state;
            state = LearnBranchingEqualityConstraint<BoolConstraint>(state, operation, falseBranch) ?? state;
            state = LearnBranchingNumberConstraint(state, operation, isLoopCondition, visitCount, falseBranch);
            state = LearnBranchingCollectionConstraint(state, operation, falseBranch);
        }
        else if (operation.OperatorKind.IsAnyRelational())
        {
            state = LearnBranchingRelationalObjectConstraint(state, operation, falseBranch) ?? state;
            state = LearnBranchingNumberConstraint(state, operation, isLoopCondition, visitCount, falseBranch);
            state = LearnBranchingCollectionConstraint(state, operation, falseBranch);
        }
        return state;
    }

    protected override ProgramState PreProcess(ProgramState state, IBinaryOperationWrapper operation)
    {
        if (state[operation.LeftOperand]?.Constraint<NumberConstraint>() is { } leftNumber
            && state[operation.RightOperand]?.Constraint<NumberConstraint>() is { } rightNumber
            && ArithmeticCalculator.Calculate(operation.OperatorKind, leftNumber, rightNumber) is { } constraint)
        {
            state = state.SetOperationConstraint(operation, constraint);
        }
        if (operation.Type.Is(KnownType.System_String) && operation.OperatorKind is BinaryOperatorKind.Add or BinaryOperatorKind.Concatenate)
        {
            state = state.SetOperationConstraint(operation, ObjectConstraint.NotNull);
        }
        return state;
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

    private static ProgramState LearnBranchingNumberConstraint(ProgramState state, IBinaryOperationWrapper binary, bool isLoopCondition, int visitCount, bool falseBranch)
    {
        var kind = falseBranch ? Opposite(binary.OperatorKind) : binary.OperatorKind;
        var leftNumber = state[binary.LeftOperand]?.Constraint<NumberConstraint>();
        var rightNumber = state[binary.RightOperand]?.Constraint<NumberConstraint>();
        if (rightNumber is not null && binary.LeftOperand.TrackedSymbol(state) is { } leftSymbol)
        {
            state = LearnBranching(leftSymbol, leftNumber, kind, rightNumber);
        }
        if (leftNumber is not null && binary.RightOperand.TrackedSymbol(state) is { } rightSymbol)
        {
            state = LearnBranching(rightSymbol, rightNumber, Flip(kind), leftNumber);
        }
        return state;

        ProgramState LearnBranching(ISymbol symbol, NumberConstraint existingNumber, BinaryOperatorKind kind, NumberConstraint comparedNumber) =>
            !(falseBranch && symbol.GetSymbolType().IsNullableValueType())  // Don't learn opposite for "nullable > 0", because it could also be <null>.
            && RelationalNumberConstraint(existingNumber, kind, comparedNumber, isLoopCondition, visitCount) is { } newConstraint
                ? state.SetSymbolConstraint(symbol, newConstraint)
                : state;

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

    private static NumberConstraint RelationalNumberConstraint(NumberConstraint existingNumber, BinaryOperatorKind kind, NumberConstraint comparedNumber, bool isLoopCondition, int visitCount)
    {
        return kind switch
        {
            BinaryOperatorKind.Equals => NumberConstraint.From(ArithmeticCalculator.BiggestMinimum(comparedNumber, existingNumber), ArithmeticCalculator.SmallestMaximum(comparedNumber, existingNumber)),
            BinaryOperatorKind.NotEquals when comparedNumber.IsSingleValue && comparedNumber.Min == existingNumber?.Min => NumberConstraint.From(existingNumber.Min + 1, existingNumber.Max),
            BinaryOperatorKind.NotEquals when comparedNumber.IsSingleValue && comparedNumber.Min == existingNumber?.Max => NumberConstraint.From(existingNumber.Min, existingNumber.Max - 1),
            BinaryOperatorKind.GreaterThan when comparedNumber.Min.HasValue => From(comparedNumber.Min + 1, null),
            BinaryOperatorKind.GreaterThanOrEqual when comparedNumber.Min.HasValue => From(comparedNumber.Min, null),
            BinaryOperatorKind.LessThan when comparedNumber.Max.HasValue => From(null, comparedNumber.Max - 1),
            BinaryOperatorKind.LessThanOrEqual when comparedNumber.Max.HasValue => From(null, comparedNumber.Max),
            _ => null
        };

        NumberConstraint From(BigInteger? newMin, BigInteger? newMax)
        {
            if (existingNumber is not null)
            {
                if ((newMin is null || (existingNumber.Min > newMin && EvaluateBranchingCondition(isLoopCondition, visitCount))) && !(existingNumber.Min > newMax))
                {
                    newMin = existingNumber.Min;
                }
                if ((newMax is null || (existingNumber.Max < newMax && EvaluateBranchingCondition(isLoopCondition, visitCount))) && !(existingNumber.Max < newMin))
                {
                    newMax = existingNumber.Max;
                }
            }
            return NumberConstraint.From(newMin, newMax);
        }
    }

    private static SymbolicConstraint BinaryOperandConstraint<T>(ProgramState state, IBinaryOperationWrapper binary) where T : SymbolicConstraint =>
        state[binary.LeftOperand]?.Constraint<T>() ?? state[binary.RightOperand]?.Constraint<T>();

    private static ISymbol BinaryOperandSymbolWithoutConstraint<T>(ProgramState state, IBinaryOperationWrapper binary) where T : SymbolicConstraint =>
        OperandSymbolWithoutConstraint<T>(state, binary.LeftOperand) ?? OperandSymbolWithoutConstraint<T>(state, binary.RightOperand);

    private static ISymbol OperandSymbolWithoutConstraint<T>(ProgramState state, IOperation candidate) where T : SymbolicConstraint =>
        candidate.TrackedSymbol(state) is { } symbol
        && (state[symbol] is null || !state[symbol].HasConstraint<T>())
            ? symbol
            : null;

    private static SymbolicConstraint BinaryConstraint(BinaryOperatorKind kind, SymbolicValue left, SymbolicValue right, bool isLoopCondition, int visitCount)
    {
        var leftBool = left?.Constraint<BoolConstraint>();
        var rightBool = right?.Constraint<BoolConstraint>();
        var leftIsNull = left?.Constraint<ObjectConstraint>() == ObjectConstraint.Null;
        var rightIsNull = right?.Constraint<ObjectConstraint>() == ObjectConstraint.Null;
        if (leftBool is null ^ rightBool is null)
        {
            return kind switch
            {
                BinaryOperatorKind.Equals when leftIsNull || rightIsNull => BoolConstraint.False,
                BinaryOperatorKind.NotEquals when leftIsNull || rightIsNull => BoolConstraint.True,
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
            && EvaluateBranchingCondition(isLoopCondition, visitCount))
        {
            return BinaryNumberConstraint(kind, leftNumber, rightNumber);
        }
        else if (left?.HasConstraint<ObjectConstraint>() is true && right?.HasConstraint<ObjectConstraint>() is true)
        {
            return BinaryNullConstraint(kind, leftIsNull, rightIsNull);
        }
        else if (leftIsNull || rightIsNull)
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
            BinaryOperatorKind.Equals when right.Min > left.Max => BoolConstraint.False,
            BinaryOperatorKind.Equals when left.Min > right.Max => BoolConstraint.False,
            BinaryOperatorKind.NotEquals when left.IsSingleValue && right.IsSingleValue => BoolConstraint.From(!left.Equals(right)),
            BinaryOperatorKind.NotEquals when left.IsSingleValue && !right.CanContain(left.Min.Value) => BoolConstraint.True,
            BinaryOperatorKind.NotEquals when right.IsSingleValue && !left.CanContain(right.Min.Value) => BoolConstraint.True,
            BinaryOperatorKind.NotEquals when right.Min > left.Max => BoolConstraint.True,
            BinaryOperatorKind.NotEquals when left.Min > right.Max => BoolConstraint.True,
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

    // Fixed loops:
    // 1st visit decides on the initial value. We don't learn from binary comparison.
    // 2nd visit does not decide on the current value. It learns range from binary comparison instead to be able to exit the loop.
    private static bool EvaluateBranchingCondition(bool isLoopCondition, int visitCount) =>
        visitCount == 1 || !isLoopCondition;
}
