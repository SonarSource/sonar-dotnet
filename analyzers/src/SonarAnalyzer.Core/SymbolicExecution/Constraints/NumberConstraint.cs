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
using System.Numerics;

namespace SonarAnalyzer.SymbolicExecution.Constraints;

public sealed class NumberConstraint : SymbolicConstraint
{
    private const int CacheLimit = 100_000; // Measured projects produced around 1000 distinct values.

    private static ConcurrentDictionary<CacheKey, NumberConstraint> cache = new();

    public BigInteger? Min { get; }
    public BigInteger? Max { get; }
    public bool IsSingleValue => Min.HasValue && Min == Max;
    public bool IsPositive => Min >= 0;
    public bool IsNegative => Max < 0;
    public bool CanBePositive => !IsNegative;
    public bool CanBeNegative => !IsPositive;
    public override bool CacheEnabled => false;

    public override SymbolicConstraint Opposite =>
        null;

    private NumberConstraint(BigInteger? min, BigInteger? max) : base(ConstraintKind.Number)
    {
        Min = min;
        Max = max;
    }

    public static NumberConstraint From(BigInteger value) =>
        From(value, value);

    public static NumberConstraint From(object value) =>
        value switch
        {
            sbyte v => From(new BigInteger(v)),
            byte v => From(new BigInteger(v)),
            short v => From(new BigInteger(v)),
            ushort v => From(new BigInteger(v)),
            int v => From(new BigInteger(v)),
            uint v => From(new BigInteger(v)),
            long v => From(new BigInteger(v)),
            ulong v => From(new BigInteger(v)),
            _ => null
        };

    public static NumberConstraint From(BigInteger? min, BigInteger? max)
    {
        if (!min.HasValue && !max.HasValue)
        {
            return null;
        }
        if (min.HasValue && max.HasValue && min.Value > max.Value)
        {
            (min, max) = (max, min);
        }
        if (cache.Count > CacheLimit)
        {
            ResetCache();
        }
        var key = new CacheKey(min, max);
        return cache.TryGetValue(key, out var constraint)
            ? constraint
            : cache.GetOrAdd(key, new NumberConstraint(min, max));
    }

    public bool CanContain(BigInteger value) =>
        !(value < Min || Max < value);

    public bool Overlaps(NumberConstraint other) =>
        other is null   // NumberConstraint.From(null, null) returns null and should be considered an unlimited range that overlaps with every other range
        || ((Min is null || other.Max is null || Min <= other.Max)
            && (Max is null || other.Min is null || Max >= other.Min));

    public override bool Equals(object obj) =>
        obj is NumberConstraint other && other.Min == Min && other.Max == Max;

    public override int GetHashCode() =>
        HashCode.Combine(Min?.GetHashCode() ?? 0, Max?.GetHashCode() ?? 0);

    public override string ToString() =>
        Min.HasValue && Min == Max ? $"{Kind} {Min}" : $"{Kind} from {Serialize(Min)} to {Serialize(Max)}";

    public static void ResetCache() =>
        cache.Clear();

    private static string Serialize(BigInteger? value) =>
        value.HasValue ? value.Value.ToString() : "*";

    private readonly record struct CacheKey(BigInteger? Min, BigInteger? Max);
}
