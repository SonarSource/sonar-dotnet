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
using SonarAnalyzer.Common;

namespace SonarAnalyzer.SymbolicExecution.Constraints
{
    public class SymbolicValueConstraints
    {
        private readonly Dictionary<Type, SymbolicValueConstraint> constraints
            = new Dictionary<Type, SymbolicValueConstraint>();

        private readonly int hashCode;

        public static SymbolicValueConstraints Create(SymbolicValueConstraint constraint)
        {
            return new SymbolicValueConstraints(constraint);
        }

        private SymbolicValueConstraints(SymbolicValueConstraint constraint)
        {
            SetConstraint(constraint, this.constraints);
            hashCode = ComputeHashcode();
        }

        private SymbolicValueConstraints(Dictionary<Type, SymbolicValueConstraint> constraints)
        {
            this.constraints = constraints;
            hashCode = ComputeHashcode();
        }

        internal IEnumerable<SymbolicValueConstraint> GetConstraints() => constraints.Values;

        internal SymbolicValueConstraints WithConstraint(SymbolicValueConstraint constraint)
        {
            var constraintsCopy = new Dictionary<Type, SymbolicValueConstraint>(constraints);
            SetConstraint(constraint, constraintsCopy);

            return new SymbolicValueConstraints(constraintsCopy);
        }

        internal SymbolicValueConstraints WithoutConstraint(SymbolicValueConstraint constraint)
        {
            var constraintsCopy = new Dictionary<Type, SymbolicValueConstraint>(constraints);
            if (constraintsCopy.Remove(constraint.GetType()))
            {
                return new SymbolicValueConstraints(constraintsCopy);
            }
            return this;
        }

        private static void SetConstraint(SymbolicValueConstraint constraint,
            Dictionary<Type, SymbolicValueConstraint> constraints)
        {
            constraints[constraint.GetType()] = constraint;

            if (constraint is BoolConstraint ||
                constraint is DisposableConstraint)
            {
                constraints[typeof(ObjectConstraint)] = ObjectConstraint.NotNull;
                if (constraints.ContainsKey(typeof(NullableValueConstraint)))
                {
                    constraints[typeof(NullableValueConstraint)] = NullableValueConstraint.HasValue;
                }
            }
        }

        internal T GetConstraintOrDefault<T>()
            where T : SymbolicValueConstraint
        {
            SymbolicValueConstraint constraint = null;
            return constraints.TryGetValue(typeof(T), out constraint)
                ? (T)constraint
                : null;
        }

        internal SymbolicValueConstraint GetConstraintOrDefault(Type constraintType)
        {
            SymbolicValueConstraint constraint = null;
            return constraints.TryGetValue(constraintType, out constraint)
                ? constraint
                : null;
        }


        internal bool HasConstraint(SymbolicValueConstraint constraint)
        {
            SymbolicValueConstraint storedConstraint;
            return constraints.TryGetValue(constraint.GetType(), out storedConstraint) &&
                   storedConstraint == constraint;
        }

        internal bool HasConstraint<T>()
        {
            SymbolicValueConstraint storedConstraint;
            return constraints.TryGetValue(typeof(T), out storedConstraint);
        }

        private int ComputeHashcode()
        {
            int hash = 17 * constraints.Count;

            foreach (var item in constraints)
            {
                hash = hash * 23 + item.Value.GetHashCode();
            }

            return hash;
        }

        public override int GetHashCode()
        {
            return hashCode;
        }

        public override bool Equals(object obj)
        {
            var other = obj as SymbolicValueConstraints;
            return other != null &&
                DictionaryHelper.DictionaryEquals(constraints, other.constraints);
        }

        // for debugging
        public override string ToString()
        {
            return string.Join(", ", constraints.Values);
        }
    }
}
