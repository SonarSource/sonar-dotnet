/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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

using System;
using System.Collections.Generic;
using System.Linq;
using SonarAnalyzer.SymbolicExecution.SymbolicValues;

namespace SonarAnalyzer.SymbolicExecution.Relationships
{
    public sealed class ComparisonRelationship : BinaryRelationship, IEquatable<ComparisonRelationship>
    {
        private readonly Lazy<int> hash;

        internal SymbolicComparisonKind ComparisonKind { get; }

        public ComparisonRelationship(SymbolicComparisonKind comparisonKind, SymbolicValue leftOperand, SymbolicValue rightOperand)
            : base(leftOperand, rightOperand)
        {
            ComparisonKind = comparisonKind;

            hash = new Lazy<int>(() =>
            {
                var h = 19;
                h = h * 31 + ComparisonKind.GetHashCode();
                h = h * 31 + base.GetHashCode();
                return h;
            });
        }

        public override BinaryRelationship Negate()
        {
            var otherComparisonKind = ComparisonKind == SymbolicComparisonKind.Less
                ? SymbolicComparisonKind.LessOrEqual
                : SymbolicComparisonKind.Less;

            return new ComparisonRelationship(otherComparisonKind, RightOperand, LeftOperand);
        }

        internal override bool IsContradicting(IEnumerable<BinaryRelationship> relationships)
        {
            // a < b and a <= b contradicts b < a
            var isLessOpContradicting = relationships
                .OfType<ComparisonRelationship>()
                .Where(c => c.ComparisonKind == SymbolicComparisonKind.Less)
                .Any(rel => AreOperandsSwapped(rel));

            if (isLessOpContradicting)
            {
                return true;
            }

            if (ComparisonKind == SymbolicComparisonKind.Less)
            {
                // a < b contradicts b <= a
                var isLessEqualOpContradicting = relationships
                    .OfType<ComparisonRelationship>()
                    .Where(c => c.ComparisonKind == SymbolicComparisonKind.LessOrEqual)
                    .Any(rel => AreOperandsSwapped(rel));

                if (isLessEqualOpContradicting)
                {
                    return true;
                }

                // a < b contradicts a == b and b == a
                var isEqualOpContradicting = relationships
                    .OfType<EqualsRelationship>()
                    .Any(rel => AreOperandsMatching(rel));

                if (isEqualOpContradicting)
                {
                    return true;
                }
            }

            if (ComparisonKind == SymbolicComparisonKind.LessOrEqual)
            {
                // a <= b contradicts a >= b && a != b
                var isLessEqualOp = relationships
                    .OfType<ComparisonRelationship>()
                    .Where(c => c.ComparisonKind == SymbolicComparisonKind.LessOrEqual)
                    .Any(rel => AreOperandsSwapped(rel));

                var isNotEqualOpContradicting = relationships
                    .OfType<ValueNotEqualsRelationship>()
                    .Any(rel => AreOperandsMatching(rel));

                if (isLessEqualOp && isNotEqualOpContradicting)
                {
                    return true;
                }
            }

            return false;
        }

        internal override IEnumerable<BinaryRelationship> GetTransitiveRelationships(IEnumerable<BinaryRelationship> relationships)
        {
            foreach (var other in relationships)
            {
                if (other is ComparisonRelationship comparison)
                {
                    var transitive = GetTransitiveRelationship(comparison);
                    if (transitive != null)
                    {
                        yield return transitive;
                    }
                }

                if (other is EqualsRelationship equals)
                {
                    var transitive = GetTransitiveRelationship(equals);
                    if (transitive != null)
                    {
                        yield return transitive;
                    }
                }
            }
        }

        private ComparisonRelationship GetTransitiveRelationship(ComparisonRelationship other)
        {
            var comparisonKind = ComparisonKind == SymbolicComparisonKind.LessOrEqual && other.ComparisonKind == SymbolicComparisonKind.LessOrEqual
                    ? SymbolicComparisonKind.LessOrEqual
                    : SymbolicComparisonKind.Less;

            if (RightOperand.Equals(other.LeftOperand))
            {
                return new ComparisonRelationship(comparisonKind, LeftOperand, other.RightOperand);
            }
            else if (LeftOperand.Equals(other.RightOperand))
            {
                return new ComparisonRelationship(comparisonKind, other.LeftOperand, RightOperand);
            }
            else
            {
                return null;
            }
        }

        private BinaryRelationship GetTransitiveRelationship(EqualsRelationship other)
        {
            if (LeftOperand.Equals(other.LeftOperand))
            {
                return new ComparisonRelationship(ComparisonKind, other.RightOperand, RightOperand);
            }
            else if (RightOperand.Equals(other.LeftOperand))
            {
                return new ComparisonRelationship(ComparisonKind, LeftOperand, other.RightOperand);
            }
            else if (LeftOperand.Equals(other.RightOperand))
            {
                return new ComparisonRelationship(ComparisonKind, other.LeftOperand, RightOperand);
            }
            else if (RightOperand.Equals(other.RightOperand))
            {
                return new ComparisonRelationship(ComparisonKind, LeftOperand, other.LeftOperand);
            }
            else
            {
                return null;
            }
        }

        internal bool AreOperandsSwapped(ComparisonRelationship rel) =>
            LeftOperand.Equals(rel.RightOperand) && RightOperand.Equals(rel.LeftOperand);

        public override string ToString()
            => $"{(ComparisonKind == SymbolicComparisonKind.Less ? "<" : "<=")}({LeftOperand}, {RightOperand})";

        public override bool Equals(object obj) =>
            obj != null && Equals(obj as ComparisonRelationship);

        public bool Equals(ComparisonRelationship other) =>
            other != null && ComparisonKind == other.ComparisonKind && base.Equals(other);

        public override int GetHashCode() => this.hash.Value;

        internal override BinaryRelationship CreateNew(SymbolicValue leftOperand, SymbolicValue rightOperand) =>
            new ComparisonRelationship(ComparisonKind, leftOperand, rightOperand);
    }
}
