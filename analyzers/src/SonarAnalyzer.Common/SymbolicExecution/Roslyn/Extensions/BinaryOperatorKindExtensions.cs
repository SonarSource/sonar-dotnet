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

namespace SonarAnalyzer.SymbolicExecution.Roslyn;

internal static class BinaryOperatorKindExtensions
{
    public static SymbolicConstraint BinaryNumberConstraint(this BinaryOperatorKind kind, NumberConstraint left, NumberConstraint right) =>
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
}
