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

using System;
using System.Collections.Generic;
using SonarAnalyzer.SymbolicExecution.SymbolicValues;

namespace SonarAnalyzer.SymbolicExecution.Relationships
{
    public abstract class BinaryRelationship : IEquatable<BinaryRelationship>
    {
        internal SymbolicValue LeftOperand { get; }
        internal SymbolicValue RightOperand { get; }

        protected BinaryRelationship(SymbolicValue leftOperand, SymbolicValue rightOperand)
        {
            LeftOperand = leftOperand;
            RightOperand = rightOperand;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            return Equals(obj as BinaryRelationship);
        }

        public bool Equals(BinaryRelationship other)
        {
            if (other == null ||
                GetType() != other.GetType())
            {
                return false;
            }

            return LeftOperand.Equals(other.LeftOperand) && RightOperand.Equals(other.RightOperand);
        }

        public override int GetHashCode()
        {
            var hash = 19;
            hash = hash * 31 + GetType().GetHashCode();
            hash = hash * 31 + LeftOperand.GetHashCode();
            hash = hash * 31 + RightOperand.GetHashCode();
            return hash;
        }

        internal abstract BinaryRelationship CreateNew(SymbolicValue leftOperand, SymbolicValue rightOperand);

        internal abstract bool IsContradicting(IEnumerable<BinaryRelationship> relationships);

        public abstract BinaryRelationship Negate();

        internal bool AreOperandsMatching(BinaryRelationship other)
        {
            return LeftOperand.Equals(other.LeftOperand) && RightOperand.Equals(other.RightOperand) ||
                RightOperand.Equals(other.LeftOperand) && LeftOperand.Equals(other.RightOperand);
        }

        internal abstract IEnumerable<BinaryRelationship> GetTransitiveRelationships(IEnumerable<BinaryRelationship> relationships);

        protected BinaryRelationship ComputeTransitiveRelationship(BinaryRelationship other, BinaryRelationship factory)
        {
            if (LeftOperand.Equals(other.LeftOperand))
            {
                return factory.CreateNew(RightOperand, other.RightOperand);
            }
            else if (RightOperand.Equals(other.LeftOperand))
            {
                return factory.CreateNew(LeftOperand, other.RightOperand);
            }
            else if (LeftOperand.Equals(other.RightOperand))
            {
                return factory.CreateNew(other.LeftOperand, RightOperand);
            }
            else if (RightOperand.Equals(other.RightOperand))
            {
                return factory.CreateNew(other.LeftOperand, LeftOperand);
            }
            else
            {
                return null;
            }
        }
    }
}
