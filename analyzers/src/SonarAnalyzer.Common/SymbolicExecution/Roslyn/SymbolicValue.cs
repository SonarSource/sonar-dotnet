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
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.SymbolicExecution.Constraints;

namespace SonarAnalyzer.SymbolicExecution.Roslyn
{
    public sealed record SymbolicValue
    {
        // Reuse instances to save memory. This "True" has the same semantic meaning and any other symbolic value with BoolConstraint.True constraint
        public static readonly SymbolicValue This = new SymbolicValue().WithConstraint(ObjectConstraint.NotNull);
        public static readonly SymbolicValue Null = new SymbolicValue().WithConstraint(ObjectConstraint.Null);
        public static readonly SymbolicValue NotNull = new SymbolicValue().WithConstraint(ObjectConstraint.NotNull);
        public static readonly SymbolicValue True = new SymbolicValue().WithConstraint(BoolConstraint.True);
        public static readonly SymbolicValue False = new SymbolicValue().WithConstraint(BoolConstraint.False);

        // SymbolicValue can have only one constraint instance of specific type at a time
        private ImmutableDictionary<Type, SymbolicConstraint> Constraints { get; init; } = ImmutableDictionary<Type, SymbolicConstraint>.Empty;

        public override string ToString() =>
            SerializeConstraints();

        public SymbolicValue WithConstraint(SymbolicConstraint constraint) =>
            this with { Constraints = Constraints.SetItem(constraint.GetType(), constraint) };

        public SymbolicValue WithoutConstraint(SymbolicConstraint constraint) =>
            HasConstraint(constraint)
                ? this with { Constraints = Constraints.Remove(constraint.GetType()) }
                : this;

        public SymbolicValue WithoutConstraint(params SymbolicConstraint[] constraint) =>
                this with { Constraints = Constraints.RemoveRange(constraint.Select(x => GetType())) };

        public SymbolicValue WithoutConstraint<T>() where T : SymbolicConstraint =>
            HasConstraint<T>()
                ? this with { Constraints = Constraints.Remove(typeof(T)) }
                : this;

        public bool HasConstraint<T>() where T : SymbolicConstraint =>
            Constraints.ContainsKey(typeof(T));

        public bool HasConstraint(SymbolicConstraint constraint) =>
            Constraints.TryGetValue(constraint.GetType(), out var current) && constraint == current;

        public T Constraint<T>() where T : SymbolicConstraint =>
            Constraints.TryGetValue(typeof(T), out var value) ? (T)value : null;

        public IEnumerable<SymbolicConstraint> AllConstraints() =>
            Constraints.Values;

        public override int GetHashCode() =>
            HashCode.DictionaryContentHash(Constraints);

        public bool Equals(SymbolicValue other) =>
            other is not null && other.Constraints.DictionaryEquals(Constraints);

        private string SerializeConstraints() =>
            Constraints.Any()
                ? Constraints.Values.Select(x => x.ToString()).OrderBy(x => x).JoinStr(", ")
                : "No constraints";
    }
}
