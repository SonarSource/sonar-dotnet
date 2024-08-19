/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

namespace SonarAnalyzer.SymbolicExecution.Roslyn;

public sealed record SymbolicValue
{
    private static ConcurrentDictionary<CacheKey, SymbolicValue> cache = new();

    // Reuse instances to save memory. This "True" has the same semantic meaning and any other symbolic value with BoolConstraint.True constraint
    public static readonly SymbolicValue Empty = new();
    public static readonly SymbolicValue Null = Empty.WithConstraint(ObjectConstraint.Null);
    public static readonly SymbolicValue NotNull = Empty.WithConstraint(ObjectConstraint.NotNull);
    public static readonly SymbolicValue True = NotNull.WithConstraint(BoolConstraint.True);
    public static readonly SymbolicValue False = NotNull.WithConstraint(BoolConstraint.False);

    private readonly ImmutableDictionary<Type, SymbolicConstraint> constraints = ImmutableDictionary<Type, SymbolicConstraint>.Empty;
    private int? hashCode;

    // SymbolicValue can have only one constraint instance of specific type at a time
    private ImmutableDictionary<Type, SymbolicConstraint> Constraints
    {
        get => constraints;
        init
        {
            constraints = value;
            hashCode = null;
        }
    }

    public IEnumerable<SymbolicConstraint> AllConstraints =>
        Constraints.Values;

    private SymbolicValue() { }

    public override string ToString() =>
        SerializeConstraints();

    public SymbolicValue WithConstraint(SymbolicConstraint constraint)
    {
        if (Constraints.Count == 0)
        {
            return CachedSymbolicValue(constraint);
        }
        else if (HasConstraint(constraint))
        {
            return this;
        }
        else
        {
            var containsContraintType = Constraints.ContainsKey(constraint.GetType());
            return Constraints.Count switch
            {
                1 when containsContraintType => CachedSymbolicValue(constraint),
                1 => CachedSymbolicValue(Constraints.Values.First(), constraint),
                2 when containsContraintType => CachedSymbolicValue(OtherSingleConstraint(constraint.GetType()), constraint),
                _ => this with { Constraints = Constraints.SetItem(constraint.GetType(), constraint) },
            };
        }
    }

    public SymbolicValue WithoutConstraint(SymbolicConstraint constraint) =>
        HasConstraint(constraint) ? RemoveConstraint(constraint.GetType()) : this;

    public SymbolicValue WithoutConstraint<T>() where T : SymbolicConstraint =>
        HasConstraint<T>() ? RemoveConstraint(typeof(T)) : this;

    public bool HasConstraint<T>() where T : SymbolicConstraint =>
        Constraints.ContainsKey(typeof(T));

    public bool HasConstraint(SymbolicConstraint constraint) =>
        Constraints.TryGetValue(constraint.GetType(), out var current) && constraint == current;

    public T Constraint<T>() where T : SymbolicConstraint =>
        Constraints.TryGetValue(typeof(T), out var value) ? (T)value : null;

    public override int GetHashCode() =>
        hashCode ??= HashCode.DictionaryContentHash(constraints);

    public bool Equals(SymbolicValue other) =>
        other is not null && other.Constraints.DictionaryEquals(Constraints);

    private string SerializeConstraints() =>
        Constraints.Any()
            ? Constraints.Values.Select(x => x.ToString()).OrderBy(x => x).JoinStr(", ")
            : "No constraints";

    private SymbolicValue RemoveConstraint(Type type) =>
        Constraints.Count switch
        {
            1 => null,
            2 => OtherSingle(type),
            3 => OtherPair(type),
            _ => this with { Constraints = Constraints.Remove(type) },
        };

    private SymbolicValue OtherSingle(Type except) =>
        CachedSymbolicValue(OtherSingleConstraint(except));

    private SymbolicConstraint OtherSingleConstraint(Type except)
    {
        // Performance: Don't use LINQ here as it neglects any gains of the caching
        foreach (var kvp in Constraints)
        {
            if (kvp.Key != except)
            {
                return kvp.Value;
            }
        }

        throw new InvalidOperationException("Unreachable. This method is called when there is exactly one other value present.");
    }

    private SymbolicValue OtherPair(Type except)
    {
        // Performance: Don't use LINQ here as it neglects any gains of the caching
        SymbolicConstraint first = null;
        foreach (var kvp in Constraints)
        {
            if (kvp.Key != except)
            {
                if (first == null)
                {
                    first = kvp.Value;
                }
                else
                {
                    return CachedSymbolicValue(first, kvp.Value);
                }
            }
        }

        throw new InvalidOperationException("Unreachable. This method is called when there are exactly two other value present.");
    }

    private static SymbolicValue CachedSymbolicValue(SymbolicConstraint first, SymbolicConstraint second = null)
    {
        if (first.CacheEnabled && (second is null || second.CacheEnabled))
        {
            // Performance: Don't use the factory overload of GetOrAdd
            var cacheKey = new CacheKey(first.Kind, second?.Kind);
            return cache.TryGetValue(cacheKey, out var result)
                ? result
                : cache.GetOrAdd(cacheKey, CreateSymbolicValue(first, second));
        }
        else
        {
            return CreateSymbolicValue(first, second);
        }
    }

    private static SymbolicValue CreateSymbolicValue(SymbolicConstraint first, SymbolicConstraint second = null)
    {
        if (second is null)
        {
            return Empty with { Constraints = Empty.Constraints.SetItem(first.GetType(), first) };
        }
        else
        {
            var firstValue = CachedSymbolicValue(first);
            return firstValue with { Constraints = firstValue.Constraints.SetItem(second.GetType(), second) };
        }
    }

    private readonly record struct CacheKey
    {
        private readonly ConstraintKind first;
        private readonly ConstraintKind? second;

        public CacheKey(ConstraintKind first, ConstraintKind? second)
        {
            if (first == second)
            {
                this.first = first;
                this.second = null;
            }
            else if (first < second || second == null)
            {
                this.first = first;
                this.second = second;
            }
            else
            {
                this.first = second.Value;
                this.second = first;
            }
        }
    }
}
