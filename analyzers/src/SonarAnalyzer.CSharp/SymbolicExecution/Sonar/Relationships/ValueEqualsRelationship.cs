﻿/*
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

using SonarAnalyzer.SymbolicExecution.Sonar.SymbolicValues;

namespace SonarAnalyzer.SymbolicExecution.Sonar.Relationships
{
    public sealed class ValueEqualsRelationship : EqualsRelationship
    {
        public ValueEqualsRelationship(SymbolicValue leftOperand, SymbolicValue rightOperand)
            : base(leftOperand, rightOperand)
        {
        }

        internal override bool IsContradicting(IEnumerable<BinaryRelationship> relationships)
        {
            var isNotEqContradicting = relationships
                .OfType<ValueNotEqualsRelationship>()
                .Any(rel => AreOperandsMatching(rel));

            if (isNotEqContradicting)
            {
                return true;
            }

            var isComparisonContradicting = relationships
                .OfType<ComparisonRelationship>()
                .Where(c => c.ComparisonKind == SymbolicComparisonKind.Less)
                .Any(c => AreOperandsMatching(c));

            return isComparisonContradicting;
        }

        public override BinaryRelationship Negate() =>
            new ValueNotEqualsRelationship(LeftOperand, RightOperand);

        public override string ToString() => $"Eq({LeftOperand}, {RightOperand})";

        internal override BinaryRelationship CreateNew(SymbolicValue leftOperand, SymbolicValue rightOperand) =>
            new ValueEqualsRelationship(leftOperand, rightOperand);

        internal override IEnumerable<BinaryRelationship> GetTransitiveRelationships(IEnumerable<BinaryRelationship> relationships)
        {
            foreach (var other in relationships)
            {
                if (other is EqualsRelationship equals)
                {
                    var transitive = ComputeTransitiveRelationship(equals, this);
                    if (transitive != null)
                    {
                        yield return transitive;
                    }
                }

                if (other is ComparisonRelationship ||
                    other is ValueNotEqualsRelationship)
                {
                    var transitive = ComputeTransitiveRelationship(other, other);
                    if (transitive != null)
                    {
                        yield return transitive;
                    }
                }
            }
        }
    }
}
