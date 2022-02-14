/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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
using System.Collections.Immutable;
using System.Linq;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.SymbolicExecution.Roslyn
{
    public sealed record SymbolicValue
    {
        private readonly SymbolicValueCounter counter;
        private readonly int identifier;    // This is debug information that is intentionally excluded from GetHashCode and Equals
        // SymbolicValue can have only one constraint instance of specific type at a time
        private ImmutableDictionary<Type, SymbolicConstraint> Constraints { get; init; } = ImmutableDictionary<Type, SymbolicConstraint>.Empty;

        public SymbolicValue(SymbolicValueCounter counter)
        {
            this.counter = counter;
            identifier = counter.NextIdentifier();
        }

        protected SymbolicValue(SymbolicValue original) // Custom record copying constructor
        {
            counter = original.counter;
            identifier = counter.NextIdentifier();
            Constraints = original.Constraints;
        }

        public override string ToString() =>
            $"SV_{identifier}{SerializeConstraints()}";

        public SymbolicValue WithConstraint(SymbolicConstraint constraint) =>
            this with { Constraints = Constraints.SetItem(constraint.GetType(), constraint) };

        public SymbolicValue WithoutConstraint(SymbolicConstraint constraint) =>
            HasConstraint(constraint)
                ? this with { Constraints = Constraints.Remove(constraint.GetType()) }
                : this;

        public bool HasConstraint<T>() where T : SymbolicConstraint =>
            Constraints.ContainsKey(typeof(T));

        public bool HasConstraint(SymbolicConstraint constraint) =>
            Constraints.TryGetValue(constraint.GetType(), out var current) && constraint == current;

        public override int GetHashCode() =>
            HashCode.DictionaryContentHash(Constraints);

        public bool Equals(SymbolicValue other) =>
            other is not null && other.Constraints.DictionaryEquals(Constraints);

        private string SerializeConstraints() =>
            Constraints.Any()
                ? ": " + Constraints.Values.Select(x => x.ToString()).OrderBy(x => x).JoinStr(", ")
                : null;
    }
}
