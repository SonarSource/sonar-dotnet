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
using SonarAnalyzer.Extensions;
using SonarAnalyzer.SymbolicExecution.Constraints;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.SymbolicExecution.Roslyn.OperationProcessors
{
    internal static class Binary
    {
        public static ProgramState Process(SymbolicContext context, IBinaryOperationWrapper binary) =>
            BinaryConstraint(binary.OperatorKind, context.State[binary.LeftOperand], context.State[binary.RightOperand]) is { } newConstraint
                ? context.SetOperationConstraint(newConstraint)
                : context.State;

        public static ProgramState LearnBranchingConstraint(ProgramState state, IBinaryOperationWrapper binary, bool useOpposite) =>
            binary.OperatorKind.IsAnyEquality()
                ? LearnBranchingConstraint<ObjectConstraint>(state, binary, useOpposite) ?? LearnBranchingConstraint<BoolConstraint>(state, binary, useOpposite) ?? state
                : state;

        private static ProgramState LearnBranchingConstraint<T>(ProgramState state, IBinaryOperationWrapper binary, bool useOpposite)
            where T : SymbolicConstraint
        {
            // We can fall through ?? because "constraint" and "testedSymbol" are exclusive. Symbols with the constraint will be recognized as "constraint" side.
            if ((OperandConstraint(binary.LeftOperand) ?? OperandConstraint(binary.RightOperand)) is { } constraint
                && (OperandSymbolWithoutConstraint(binary.LeftOperand) ?? OperandSymbolWithoutConstraint(binary.RightOperand)) is { } testedSymbol)
            {
                constraint = constraint.ApplyOpposite(useOpposite ^ binary.OperatorKind.IsNotEquals());
                return constraint is null ? null : state.SetSymbolConstraint(testedSymbol, constraint);
            }
            else
            {
                return null;
            }

            ISymbol OperandSymbolWithoutConstraint(IOperation candidate) =>
                candidate.TrackedSymbol() is { } symbol
                && (state[symbol] is null || !state[symbol].HasConstraint<T>())
                    ? symbol
                    : null;

            SymbolicConstraint OperandConstraint(IOperation candidate) =>
                state[candidate] is { } value && value.HasConstraint<T>()
                    ? value.Constraint<T>()
                    : null;
        }

        private static SymbolicConstraint BinaryConstraint(BinaryOperatorKind kind, SymbolicValue left, SymbolicValue right)
        {
            if (left is null && right is null)
            {
                return null;
            }
            else if (left is null || right is null)
            {
                return kind switch
                {
                    BinaryOperatorKind.Or when (left ?? right).HasConstraint(BoolConstraint.True) => BoolConstraint.True,
                    BinaryOperatorKind.And when (left ?? right).HasConstraint(BoolConstraint.False) => BoolConstraint.False,
                    _ => null
                };
            }
            else if (left.HasConstraint<BoolConstraint>() && right.HasConstraint<BoolConstraint>())
            {
                return BinaryBoolConstraint(kind, left.HasConstraint(BoolConstraint.True), right.HasConstraint(BoolConstraint.True));
            }
            else if (left.HasConstraint<ObjectConstraint>() && right.HasConstraint<ObjectConstraint>())
            {
                return BinaryNullConstraint(kind, left.HasConstraint(ObjectConstraint.Null), right.HasConstraint(ObjectConstraint.Null));
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
                    _ => null
                }
                : null;
    }
}
