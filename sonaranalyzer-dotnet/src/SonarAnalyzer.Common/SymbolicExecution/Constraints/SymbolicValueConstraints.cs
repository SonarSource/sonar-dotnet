/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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
using System.Collections.Immutable;
using SonarAnalyzer.Common;

namespace SonarAnalyzer.SymbolicExecution.Constraints
{
    public class SymbolicValueConstraints
    {
        private readonly ImmutableDictionary<Type, SymbolicValueConstraint> constraints
            = ImmutableDictionary.Create<Type, SymbolicValueConstraint>();

        private readonly int hashCode;

        public static SymbolicValueConstraints Create(SymbolicValueConstraint constraint)
        {
            return new SymbolicValueConstraints(constraint);
        }

        private SymbolicValueConstraints(SymbolicValueConstraint constraint)
        {
            constraints = SetConstraint(constraint, ImmutableDictionary.Create<Type, SymbolicValueConstraint>());
            this.hashCode = ComputeHashcode();
        }

        private SymbolicValueConstraints(ImmutableDictionary<Type, SymbolicValueConstraint> constraints)
        {
            this.constraints = constraints;
            this.hashCode = ComputeHashcode();
        }

        internal IEnumerable<SymbolicValueConstraint> GetConstraints() => this.constraints.Values;

        internal SymbolicValueConstraints WithConstraint(SymbolicValueConstraint constraint)
        {
            return new SymbolicValueConstraints(SetConstraint(constraint, constraints));
        }

        internal SymbolicValueConstraints WithoutConstraint(SymbolicValueConstraint constraint)
        {
            return new SymbolicValueConstraints(this.constraints.Remove(constraint.GetType()));
        }

        private static ImmutableDictionary<Type, SymbolicValueConstraint> SetConstraint(SymbolicValueConstraint constraint,
            ImmutableDictionary<Type, SymbolicValueConstraint> constraints)
        {
            var newConstraints = constraints.SetItem(constraint.GetType(), constraint);

            if (constraint is BoolConstraint ||
                constraint is DisposableConstraint)
            {
                newConstraints = newConstraints.SetItem(typeof(ObjectConstraint), ObjectConstraint.NotNull);
                if (constraints.ContainsKey(typeof(NullableValueConstraint)))
                {
                    newConstraints = newConstraints.SetItem(typeof(NullableValueConstraint), NullableValueConstraint.HasValue);
                }
            }

            return newConstraints;
        }

        internal T GetConstraintOrDefault<T>()
            where T : SymbolicValueConstraint
        {
            return this.constraints.TryGetValue(typeof(T), out var constraint)
                ? (T)constraint
                : null;
        }

        internal SymbolicValueConstraint GetConstraintOrDefault(Type constraintType)
        {
            return this.constraints.TryGetValue(constraintType, out var constraint)
                ? constraint
                : null;
        }

        internal bool HasConstraint(SymbolicValueConstraint constraint)
        {
            return this.constraints.TryGetValue(constraint.GetType(), out var storedConstraint) &&
                   storedConstraint == constraint;
        }

        internal bool HasConstraint<T>()
        {
            return this.constraints.TryGetValue(typeof(T), out var storedConstraint);
        }

        private int ComputeHashcode()
        {
            var hash = 17 * this.constraints.Count;

            foreach (var item in this.constraints)
            {
                hash = hash * 23 + item.Value.GetHashCode();
            }

            return hash;
        }

        public override int GetHashCode()
        {
            return this.hashCode;
        }

        public override bool Equals(object obj)
        {
            return obj is SymbolicValueConstraints other &&
                DictionaryHelper.DictionaryEquals(this.constraints, other.constraints);
        }

        // for debugging
        public override string ToString()
        {
            return string.Join(", ", this.constraints.Values);
        }
    }
}
