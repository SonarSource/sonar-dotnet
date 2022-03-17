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

using SonarAnalyzer.SymbolicExecution.Constraints;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.SymbolicExecution.Roslyn.OperationProcessors
{
    internal static class Binary
    {
        public static ProgramState Process(SymbolicContext context, IBinaryOperationWrapper binary) =>
            context.State[binary.LeftOperand] is { } left
            && context.State[binary.RightOperand] is { } right
            && left.HasConstraint<BoolConstraint>()
            && right.HasConstraint<BoolConstraint>()
            && BinaryBoolConstraint(binary.OperatorKind, left.HasConstraint(BoolConstraint.True), right.HasConstraint(BoolConstraint.True)) is { } newConstraint
                ? context.SetOperationConstraint(newConstraint)
                : context.State;

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
    }
}
