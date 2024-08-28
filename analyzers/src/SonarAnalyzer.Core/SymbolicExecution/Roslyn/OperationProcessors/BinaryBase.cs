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

internal abstract class BinaryBase<TOperation> : BranchingProcessor<TOperation>
    where TOperation : IOperationWrapper
{
    protected static BoolConstraint BoolConstraintFromBinaryOperation(BinaryOperatorKind kind, SymbolicValue left, SymbolicValue right)
    {
        var leftBool = left?.Constraint<BoolConstraint>();
        var rightBool = right?.Constraint<BoolConstraint>();
        var leftIsNull = left?.Constraint<ObjectConstraint>() == ObjectConstraint.Null;
        var rightIsNull = right?.Constraint<ObjectConstraint>() == ObjectConstraint.Null;
        if (leftBool is not null && rightBool is not null)
        {
            return BoolConstraintFromBoolConstraints(kind, leftBool == BoolConstraint.True, rightBool == BoolConstraint.True);
        }
        else if (leftBool is null ^ rightBool is null)
        {
            return BoolConstraintFromBoolAndNullConstraints(kind, leftBool, rightBool, leftIsNull, rightIsNull);
        }
        else if (left?.Constraint<NumberConstraint>() is { } leftNumber
            && right?.Constraint<NumberConstraint>() is { } rightNumber)
        {
            return kind.BoolConstraintFromNumberConstraints(leftNumber, rightNumber);
        }
        else if (left?.HasConstraint<ObjectConstraint>() is true && right?.HasConstraint<ObjectConstraint>() is true)
        {
            return BoolConstraintFromNullConstraints(kind, leftIsNull, rightIsNull);
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

    protected static ProgramState LearnBranchingEqualityConstraint(ProgramState state, IOperation leftOperand, IOperation rightOperand, BinaryOperatorKind operatorKind, bool falseBranch)
    {
        state = LearnBranchingEqualityConstraint<ObjectConstraint>(state, leftOperand, rightOperand, operatorKind, falseBranch) ?? state;
        state = LearnBranchingEqualityConstraint<BoolConstraint>(state, leftOperand, rightOperand, operatorKind, falseBranch) ?? state;
        state = LearnBranchingNumberConstraint(state, leftOperand, rightOperand, operatorKind, falseBranch);
        state = LearnBranchingCollectionConstraint(state, leftOperand, rightOperand, operatorKind, falseBranch);
        return state;
    }

    protected static ProgramState LearnBranchingNumberConstraint(ProgramState state, IOperation leftOperand, IOperation rightOperand, BinaryOperatorKind operatorKind, bool falseBranch)
    {
        var kind = operatorKind.ApplyOpposite(falseBranch);
        var leftNumber = state[leftOperand]?.Constraint<NumberConstraint>();
        var rightNumber = state[rightOperand]?.Constraint<NumberConstraint>();
        if (rightNumber is not null && OperandSymbol(leftOperand) is { } leftSymbol)
        {
            state = LearnBranching(leftSymbol, leftNumber, kind, rightNumber);
        }
        if (leftNumber is not null && OperandSymbol(rightOperand) is { } rightSymbol)
        {
            state = LearnBranching(rightSymbol, rightNumber, Flip(kind), leftNumber);
        }
        return state;

        ISymbol OperandSymbol(IOperation operand) =>
            !operand.ConstantValue.HasValue
            && operand.TrackedSymbol(state) is { } symbol
            && symbol.GetSymbolType() is INamedTypeSymbol type
            && (type.IsAny(KnownType.IntegralNumbersIncludingNative) || type.IsNullableOfAny(KnownType.IntegralNumbersIncludingNative))
                ? symbol
                : null;

        ProgramState LearnBranching(ISymbol symbol, NumberConstraint existingNumber, BinaryOperatorKind kind, NumberConstraint comparedNumber) =>
            !(falseBranch && symbol.GetSymbolType().IsNullableValueType())  // Don't learn opposite for "nullable > 0", because it could also be <null>.
            && kind.NumberConstraintFromRelationalOperator(existingNumber, comparedNumber) is { } newConstraint
                ? state.SetSymbolConstraint(symbol, newConstraint)
                : state;
    }

    protected static ProgramState LearnBranchingCollectionConstraint(ProgramState state, IOperation leftOperand, IOperation rightOperand, BinaryOperatorKind binaryOperatorKind, bool falseBranch)
    {
        var operatorKind = binaryOperatorKind.ApplyOpposite(falseBranch);
        IOperation otherOperand;
        if (InstanceOfCountProperty(leftOperand) is { } collection)
        {
            otherOperand = rightOperand;
        }
        else
        {
            otherOperand = leftOperand;
            operatorKind = Flip(operatorKind);
            collection = InstanceOfCountProperty(rightOperand);
        }

        return collection is not null
            && state.Constraint<NumberConstraint>(otherOperand) is { } number
            && CollectionConstraintFromOperator(operatorKind, number) is { } constraint
                ? state.SetSymbolConstraint(collection, constraint)
                : state;

        ISymbol InstanceOfCountProperty(IOperation operation) =>
            operation.AsPropertyReference() is { Instance: { } instance, Property.Name: nameof(Array.Length) or nameof(List<int>.Count) }
            && instance.Type.DerivesOrImplementsAny(CollectionTracker.CollectionTypes)
            && instance.TrackedSymbol(state) is { } symbol
                ? symbol
                : null;
    }

    protected static ISymbol BinaryOperandSymbolWithoutConstraint<T>(ProgramState state, IOperation leftOperand, IOperation rightOperand) where T : SymbolicConstraint =>
        OperandSymbolWithoutConstraint<T>(state, leftOperand) ?? OperandSymbolWithoutConstraint<T>(state, rightOperand);

    protected static SymbolicConstraint BinaryOperandConstraint<T>(ProgramState state, IOperation leftOperand, IOperation rightOperand) where T : SymbolicConstraint =>
        state[leftOperand]?.Constraint<T>() ?? state[rightOperand]?.Constraint<T>();

    private static ProgramState LearnBranchingEqualityConstraint<T>(ProgramState state, IOperation leftOperand, IOperation rightOperand, BinaryOperatorKind operatorKind, bool falseBranch)
        where T : SymbolicConstraint
    {
        var useOpposite = falseBranch ^ operatorKind.IsNotEquals();
        // We can take left or right constraint and "testedSymbol" because they are exclusive. Symbols with T constraint will be recognized as the constraining side.
        if (BinaryOperandConstraint<T>(state, leftOperand, rightOperand) is { } constraint
            && BinaryOperandSymbolWithoutConstraint<T>(state, leftOperand, rightOperand) is { } testedSymbol
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

    private static SymbolicConstraint CollectionConstraintFromOperator(BinaryOperatorKind operatorKind, NumberConstraint number) =>
        // consider count to be the left operand and the comparison to resolve to true:
        operatorKind switch
        {
            _ when operatorKind.IsEquals() && number.Min > 0 => CollectionConstraint.NotEmpty,
            _ when operatorKind.IsEquals() && number.Max == 0 => CollectionConstraint.Empty,
            _ when operatorKind.IsNotEquals() && number.Max == 0 => CollectionConstraint.NotEmpty,
            BinaryOperatorKind.GreaterThan when number.Min >= 0 => CollectionConstraint.NotEmpty,
            BinaryOperatorKind.GreaterThanOrEqual when number.Min >= 1 => CollectionConstraint.NotEmpty,
            BinaryOperatorKind.LessThan when number.Max == 1 => CollectionConstraint.Empty,
            BinaryOperatorKind.LessThanOrEqual when number.Max == 0 => CollectionConstraint.Empty,
            _ => null
        };

    private static BoolConstraint BoolConstraintFromBoolConstraints(BinaryOperatorKind kind, bool left, bool right) =>
         kind switch
         {
             BinaryOperatorKind.Equals or BinaryOperatorKind.ObjectValueEquals => BoolConstraint.From(left == right),
             BinaryOperatorKind.NotEquals or BinaryOperatorKind.ObjectValueNotEquals => BoolConstraint.From(left != right),
             BinaryOperatorKind.And or BinaryOperatorKind.ConditionalAnd => BoolConstraint.From(left && right),
             BinaryOperatorKind.Or or BinaryOperatorKind.ConditionalOr => BoolConstraint.From(left || right),
             BinaryOperatorKind.ExclusiveOr => BoolConstraint.From(left ^ right),
             _ => null
         };

    private static BoolConstraint BoolConstraintFromNullConstraints(BinaryOperatorKind kind, bool isNullLeft, bool isNullRight) =>
        isNullLeft || isNullRight
            ? kind switch
            {
                BinaryOperatorKind.Equals or BinaryOperatorKind.ObjectValueEquals => BoolConstraint.From(isNullLeft && isNullRight),
                BinaryOperatorKind.NotEquals or BinaryOperatorKind.ObjectValueNotEquals => BoolConstraint.From(isNullLeft != isNullRight),
                _ when kind.IsAnyRelational() => BoolConstraint.False,
                _ => null
            }
            : null;

    private static BoolConstraint BoolConstraintFromBoolAndNullConstraints(BinaryOperatorKind kind,
                                                                           BoolConstraint leftBool,
                                                                           BoolConstraint rightBool,
                                                                           bool leftIsNull,
                                                                           bool rightIsNull) =>
        kind switch
        {
            BinaryOperatorKind.Equals when leftIsNull || rightIsNull => BoolConstraint.False,
            BinaryOperatorKind.NotEquals when leftIsNull || rightIsNull => BoolConstraint.True,
            BinaryOperatorKind.Or or BinaryOperatorKind.ConditionalOr when (leftBool ?? rightBool) == BoolConstraint.True => BoolConstraint.True,
            BinaryOperatorKind.And or BinaryOperatorKind.ConditionalAnd when (leftBool ?? rightBool) == BoolConstraint.False => BoolConstraint.False,
            _ => null
        };

    private static ISymbol OperandSymbolWithoutConstraint<T>(ProgramState state, IOperation candidate) where T : SymbolicConstraint =>
        candidate.TrackedSymbol(state) is { } symbol
        && (state[symbol] is null || !state[symbol].HasConstraint<T>())
            ? symbol
            : null;

    private static BinaryOperatorKind Flip(BinaryOperatorKind kind) =>
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
}
