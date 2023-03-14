/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

using System.Collections.Concurrent;
using SonarAnalyzer.SymbolicExecution.Constraints;

namespace SonarAnalyzer.SymbolicExecution.Roslyn
{
    public sealed record SymbolicValue
    {
        // Reuse instances to save memory. This "True" has the same semantic meaning and any other symbolic value with BoolConstraint.True constraint
        public static readonly SymbolicValue Constraintless = new();
        public static readonly SymbolicValue This = Constraintless.WithConstraint(ObjectConstraint.NotNull);
        public static readonly SymbolicValue Null = Constraintless.WithConstraint(ObjectConstraint.Null);
        public static readonly SymbolicValue NotNull = Constraintless.WithConstraint(ObjectConstraint.NotNull);
        public static readonly SymbolicValue True = NotNull.WithConstraint(BoolConstraint.True);
        public static readonly SymbolicValue False = NotNull.WithConstraint(BoolConstraint.False);

        // SymbolicValue can have only one constraint instance of specific type at a time
        private ImmutableDictionary<Type, SymbolicConstraint> Constraints { get; init; } = ImmutableDictionary<Type, SymbolicConstraint>.Empty;

        public IEnumerable<SymbolicConstraint> AllConstraints =>
            Constraints.Values;

        public override string ToString() =>
            SerializeConstraints();

        public SymbolicValue WithConstraint(SymbolicConstraint constraint) =>
            AddOrReplaceConstraint(this, constraint);

        public SymbolicValue WithoutConstraint(SymbolicConstraint constraint) =>
            RemoveConstraint(this, constraint);

        public SymbolicValue WithoutConstraint<T>() where T : SymbolicConstraint =>
            RemoveConstraint<T>(this);

        public bool HasConstraint<T>() where T : SymbolicConstraint =>
            Constraints.ContainsKey(typeof(T));

        public bool HasConstraint(SymbolicConstraint constraint) =>
            Constraints.TryGetValue(constraint.GetType(), out var current) && constraint == current;

        public T Constraint<T>() where T : SymbolicConstraint =>
            Constraints.TryGetValue(typeof(T), out var value) ? (T)value : null;

        public override int GetHashCode() =>
            HashCode.DictionaryContentHash(Constraints);

        public bool Equals(SymbolicValue other) =>
            other is not null && other.Constraints.DictionaryEquals(Constraints);

        private string SerializeConstraints() =>
            Constraints.Any()
                ? Constraints.Values.Select(x => x.ToString()).OrderBy(x => x).JoinStr(", ")
                : "No constraints";

        private static SymbolicValue AddOrReplaceConstraint(SymbolicValue baseValue, SymbolicConstraint constraint)
        {
            if (baseValue.HasConstraint(constraint))
            {
                return baseValue;
            }

            var constraintCount = baseValue.Constraints.Count;
            if (constraintCount == 0 || (constraintCount == 1 && baseValue.Constraints.ContainsKey(constraint.GetType())))
            {
                return SingleConstraint(constraint);
            }

            return baseValue with { Constraints = baseValue.Constraints.SetItem(constraint.GetType(), constraint) };
        }

        private static SymbolicValue RemoveConstraint(SymbolicValue baseValue, SymbolicConstraint constraint) =>
            baseValue.HasConstraint(constraint) ? RemoveConstraint(baseValue, constraint.GetType()) : baseValue;

        private static SymbolicValue RemoveConstraint<T>(SymbolicValue baseValue) where T : SymbolicConstraint =>
            baseValue.HasConstraint<T>() ? RemoveConstraint(baseValue, typeof(T)) : baseValue;

        private static SymbolicValue RemoveConstraint(SymbolicValue baseValue, Type type)
        {
            var constraintCount = baseValue.Constraints.Count;
            if (constraintCount == 1)
            {
                return Constraintless;
            }

            if (constraintCount == 2)
            {
                SymbolicConstraint otherConstraint = null;
                foreach (var kvp in baseValue.Constraints)
                {
                    if (kvp.Key != type)
                    {
                        otherConstraint = kvp.Value;
                    }
                }
                return SingleConstraint(otherConstraint);
            }

            return baseValue with { Constraints = baseValue.Constraints.Remove(type) };
        }

        private static SymbolicValue SingleConstraint(SymbolicConstraint constraint) =>
            constraint.Kind switch
            {
                ConstraintKind.ObjectNotNull => NotNull,
                ConstraintKind.ObjectNull => Null,
                _ => null,
            } ?? Constraintless with { Constraints = Constraintless.Constraints.SetItem(constraint.GetType(), constraint) };
    }
}
