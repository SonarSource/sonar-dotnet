/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2017 SonarSource SA
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

using System.Collections.Generic;
using System.Linq;
using SonarAnalyzer.SymbolicExecution.SymbolicValues;

namespace SonarAnalyzer.SymbolicExecution.Relationships
{
    public sealed class ValueNotEqualsRelationship : NotEqualsRelationship
    {
        public ValueNotEqualsRelationship(SymbolicValue leftOperand, SymbolicValue rightOperand)
            : base(leftOperand, rightOperand)
        {
        }

        internal override bool IsContradicting(IEnumerable<BinaryRelationship> relationships)
        {
            var isEqContradicting = relationships
                .OfType<EqualsRelationship>()
                .Any(rel => AreOperandsMatching(rel));

            if (isEqContradicting)
            {
                return true;
            }

            var comparisons = relationships
                .OfType<ComparisonRelationship>()
                .Where(c => c.ComparisonKind == ComparisonKind.LessOrEqual)
                .Where(c => AreOperandsMatching(c));

            return comparisons.Count() == 2;
        }

        public override BinaryRelationship Negate()
        {
            return new ValueEqualsRelationship(LeftOperand, RightOperand);
        }

        public override string ToString()
        {
            return $"!Eq({LeftOperand}, {RightOperand})";
        }

        internal override BinaryRelationship CreateNew(SymbolicValue leftOperand, SymbolicValue rightOperand)
        {
            return new ValueNotEqualsRelationship(leftOperand, rightOperand);
        }

        internal override IEnumerable<BinaryRelationship> GetTransitiveRelationships(IEnumerable<BinaryRelationship> relationships)
        {
            return relationships
                .OfType<EqualsRelationship>()
                .Select(other => ComputeTransitiveRelationship(other, this))
                .Where(t => t != null);
        }
    }
}
