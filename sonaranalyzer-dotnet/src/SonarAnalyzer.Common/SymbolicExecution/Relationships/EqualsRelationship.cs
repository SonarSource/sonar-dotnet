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
using SonarAnalyzer.SymbolicExecution.SymbolicValues;

namespace SonarAnalyzer.SymbolicExecution.Relationships
{
    public abstract class EqualsRelationship : BinaryRelationship, IEquatable<EqualsRelationship>
    {
        protected EqualsRelationship(SymbolicValue leftOperand, SymbolicValue rightOperand)
            : base(leftOperand, rightOperand)
        {
        }

        public bool Equals(EqualsRelationship other)
        {
            return other != null && AreOperandsMatching(other);
        }

        public sealed override int GetHashCode()
        {
            var left = LeftOperand.GetHashCode();
            var right = RightOperand.GetHashCode();

            return GetHashCodeMinMaxOrdered(left, right, GetType().GetHashCode());
        }

        public sealed override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            return Equals(obj as EqualsRelationship);
        }

        internal static int GetHashCodeMinMaxOrdered(int leftHash, int rightHash, int typeHash)
        {
            var min = Math.Min(leftHash, rightHash);
            var max = Math.Max(leftHash, rightHash);

            var hash = 19;
            hash = hash * 31 + typeHash;
            hash = hash * 31 + min.GetHashCode();
            hash = hash * 31 + max.GetHashCode();
            return hash;
        }
    }
}
