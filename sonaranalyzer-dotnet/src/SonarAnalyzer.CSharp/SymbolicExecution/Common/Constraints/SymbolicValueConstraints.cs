/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2020 SonarSource SA
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
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.SymbolicExecution.Constraints
{
    public class SymbolicValueConstraints
    {
        private readonly Dictionary<Type, SymbolicValueConstraint> constraints = new Dictionary<Type, SymbolicValueConstraint>();
        private readonly int hashCode;

        public static SymbolicValueConstraints Create(SymbolicValueConstraint constraint) =>
            new SymbolicValueConstraints(constraint);

        private SymbolicValueConstraints(SymbolicValueConstraint constraint)
        {
            SetConstraint(constraint, constraints);
            hashCode = ComputeHashcode();
        }

        private SymbolicValueConstraints(Dictionary<Type, SymbolicValueConstraint> constraints)
        {
            this.constraints = constraints;
            hashCode = ComputeHashcode();
        }

        public override int GetHashCode() => hashCode;

        public override bool Equals(object obj) =>
            obj is SymbolicValueConstraints other
            && constraints.DictionaryEquals(other.constraints);

        // for debugging and error logging
        public override string ToString() =>
            string.Join(", ", constraints.Values);

        internal IEnumerable<SymbolicValueConstraint> GetConstraints() => constraints.Values;

        internal SymbolicValueConstraints WithConstraint(SymbolicValueConstraint constraint)
        {
            var constraintsCopy = new Dictionary<Type, SymbolicValueConstraint>(constraints);
            SetConstraint(constraint, constraintsCopy);

            return new SymbolicValueConstraints(constraintsCopy);
        }

        internal SymbolicValueConstraints WithoutConstraint(SymbolicValueConstraint constraint)
        {
            if (constraints.ContainsKey(constraint.GetType()))
            {
                var constraintsCopy = new Dictionary<Type, SymbolicValueConstraint>(constraints);
                constraintsCopy.Remove(constraint.GetType());
                return new SymbolicValueConstraints(constraintsCopy);
            }
            return this;
        }

        internal T GetConstraintOrDefault<T>()
            where T : SymbolicValueConstraint =>
            constraints.TryGetValue(typeof(T), out var constraint)
                ? (T)constraint
                : null;

        internal bool HasConstraint(SymbolicValueConstraint constraint) =>
            constraints.TryGetValue(constraint.GetType(), out var storedConstraint)
            && storedConstraint == constraint;

        internal bool HasConstraint<T>() =>
            constraints.ContainsKey(typeof(T));

        private static void SetConstraint(SymbolicValueConstraint constraint,
            IDictionary<Type, SymbolicValueConstraint> constraints)
        {
            constraints[constraint.GetType()] = constraint;

            if (constraint is BoolConstraint)
            {
                constraints[typeof(ObjectConstraint)] = ObjectConstraint.NotNull;
                if (constraints.ContainsKey(typeof(NullableValueConstraint)))
                {
                    constraints[typeof(NullableValueConstraint)] = NullableValueConstraint.HasValue;
                }
            }
        }

        private int ComputeHashcode()
        {
            var hash = 17 * constraints.Count;

            foreach (var item in constraints)
            {
                hash = hash * 23 + item.Value.GetHashCode();
            }

            return hash;
        }

    }
}
