/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using System.Runtime.CompilerServices;
using Roslyn.Utilities;

namespace SonarAnalyzer.Core.Common;

public static class HashCode    // Replacement for System.HashCode that is available from .NET Standard 2.1
{
    private const uint Seed = 374761393U;
    private const uint PreMultiplier = 3266489917U;
    private const uint PostMultiplier = 668265263U;
    private const int RotateOffset = 17;
    private const int IntSeed = 393241;

    [PerformanceSensitive("https://github.com/SonarSource/sonar-dotnet/pull/7012", AllowCaptures = false, AllowGenericEnumeration = false, AllowImplicitBoxing = false)]
    public static int DictionaryContentHash<TKey, TValue>(ImmutableDictionary<TKey, TValue> dictionary)
    {
        // Performance: Make sure this method is allocation free:
        // * Don't use IDictionary<TKey, TValue> as parameter, because we will not get the struct ImmutableDictionary.Enumerator but the boxed version of it
        // * Don't use Linq for the same reason.
        if (dictionary.IsEmpty)
        {
            return 0;
        }

        var seed = IntSeed;
        foreach (var kvp in dictionary)
        {
            seed ^= Combine(kvp.Key, kvp.Value);
        }
        return seed;
    }

    /// <summary>
    /// Calculates a hash for the enumerable based on the content. The same values in a different order produce the same hash-code.
    /// </summary>
    public static int EnumerableUnorderedContentHash<TValue>(IEnumerable<TValue> enumerable) =>
        enumerable.Aggregate(IntSeed, (seed, x) => seed ^ (x?.GetHashCode() ?? 0));

    /// <summary>
    /// Calculates a hash for the enumerable based on the content. The same values in a different order produce different hash-codes.
    /// </summary>
    public static int EnumerableOrderedContentHash<TValue>(IEnumerable<TValue> enumerable) =>
        enumerable.Aggregate(IntSeed, Combine);

    public static int Combine<T1, T2>(T1 a, T2 b) =>
        (int)Seed.AddHash(a?.GetHashCode()).AddHash(b?.GetHashCode());

    public static int Combine<T1, T2, T3>(T1 a, T2 b, T3 c) =>
        (int)Combine(a, b).AddHash(c?.GetHashCode());

    public static int Combine<T1, T2, T3, T4>(T1 a, T2 b, T3 c, T4 d) =>
        (int)Combine(a, b, c).AddHash(d?.GetHashCode());

    public static int Combine<T1, T2, T3, T4, T5>(T1 a, T2 b, T3 c, T4 d, T5 e) =>
        (int)Combine(a, b, c, d).AddHash(e?.GetHashCode());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint AddHash(this int hash, int? value) =>
        ((uint)hash).AddHash(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint AddHash(this uint hash, int? value) =>
        RotateLeft(hash + (uint)(value ?? 0) * PreMultiplier) * PostMultiplier;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static uint RotateLeft(uint value) =>
        (value << RotateOffset) | (value >> (sizeof(int) - RotateOffset));
}
