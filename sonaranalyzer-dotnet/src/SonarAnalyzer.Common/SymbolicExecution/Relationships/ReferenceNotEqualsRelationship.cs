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

namespace SonarAnalyzer.SymbolicExecution.Relationships
{
    public sealed class ReferenceNotEqualsRelationship : NotEqualsRelationship
    {
        public ReferenceNotEqualsRelationship(SymbolicValue leftOperand, SymbolicValue rightOperand)
            : base(leftOperand, rightOperand)
        {
        }

        internal override bool IsContradicting(IEnumerable<BinaryRelationship> relationships)
        {
            return relationships
                .OfType<ReferenceEqualsRelationship>()
                .Any(rel => AreOperandsMatching(rel));
        }

        public override BinaryRelationship Negate()
        {
            return new ReferenceEqualsRelationship(LeftOperand, RightOperand);
        }

        public override string ToString()
        {
            return $"!RefEq({LeftOperand}, {RightOperand})";
        }

        internal override BinaryRelationship CreateNew(SymbolicValue leftOperand, SymbolicValue rightOperand)
        {
            return new ReferenceNotEqualsRelationship(leftOperand, rightOperand);
        }

        internal override IEnumerable<BinaryRelationship> GetTransitiveRelationships(IEnumerable<BinaryRelationship> relationships)
        {
            return relationships
                .OfType<ReferenceEqualsRelationship>()
                .Select(other => ComputeTransitiveRelationship(other, this))
                .Where(t => t != null);
        }
    }
}
