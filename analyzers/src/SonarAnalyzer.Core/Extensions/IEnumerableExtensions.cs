/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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

namespace SonarAnalyzer.Core.Extensions;

public static class IEnumerableExtensions
{
    public static HashSet<T> ToHashSet<T>(this IEnumerable<T> enumerable, IEqualityComparer<T> equalityComparer = null)
    {
        if (enumerable is null)
        {
            return equalityComparer is null ? new() : new(equalityComparer);
        }
        else
        {
            return equalityComparer is null ? new(enumerable) : new(enumerable, equalityComparer);
        }
    }

    /// <summary>
    /// Compares each element between two collections. The elements needs in the same order to be considered equal.
    /// </summary>
    public static bool Equals<T, V>(this IEnumerable<T> first, IEnumerable<V> second, Func<T, V, bool> predicate)
    {
        var enum1 = first.GetEnumerator();
        var enum2 = second.GetEnumerator();
        var enum1HasNext = enum1.MoveNext();
        var enum2HasNext = enum2.MoveNext();

        while (enum1HasNext && enum2HasNext)
        {
            if (!predicate(enum1.Current, enum2.Current))
            {
                return false;
            }

            enum1HasNext = enum1.MoveNext();
            enum2HasNext = enum2.MoveNext();
        }

        return enum1HasNext == enum2HasNext;
    }

    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T> enumerable) where T : class =>
        enumerable.Where(x => x is not null);

    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> enumerable) where T : struct =>
        enumerable.Where(x => x.HasValue).Select(x => x.Value);

    /// <summary>
    /// Applies a specified function to the corresponding elements of two sequences,
    /// producing a sequence of the results. If the collections have different length
    /// default(T) will be passed in the operation function for the corresponding items that
    /// do not exist.
    /// </summary>
    /// <typeparam name="TFirst">The type of the elements of the first input sequence.</typeparam>
    /// <typeparam name="TSecond">The type of the elements of the second input sequence.</typeparam>
    /// <typeparam name="TResult">The type of the elements of the result sequence.</typeparam>
    /// <param name="first">The first sequence to merge.</param>
    /// <param name="second">The second sequence to merge.</param>
    /// <param name="operation">A function that specifies how to merge the elements from the two sequences.</param>
    /// <returns>An System.Collections.Generic.IEnumerable`1 that contains merged elements of
    /// two input sequences.</returns>
    public static IEnumerable<TResult> Merge<TFirst, TSecond, TResult>(this IEnumerable<TFirst> first, IEnumerable<TSecond> second, Func<TFirst, TSecond, TResult> operation)
    {
        using var iter1 = first.GetEnumerator();
        using var iter2 = second.GetEnumerator();
        while (iter1.MoveNext())
        {
            yield return operation(iter1.Current, iter2.MoveNext() ? iter2.Current : default);
        }

        while (iter2.MoveNext())
        {
            yield return operation(default, iter2.Current);
        }
    }

    public static int IndexOf<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate) =>
        enumerable.Select((item, index) => new { item, index }).FirstOrDefault(x => predicate(x.item))?.index ?? -1;

    /// <summary>
    /// This is <see cref="string.Join"/> as extension. It concatenates the members of the collection using the specified <paramref name="separator"/> between each member.
    /// <paramref name="selector"/> is used to convert <typeparamref name="T"/> to <see cref="string"/> for concatenation.
    /// </summary>
    public static string JoinStr<T>(this IEnumerable<T> enumerable, string separator, Func<T, string> selector) =>
        string.Join(separator, enumerable.Select(x => selector(x)));

    /// <summary>
    /// This is <see cref="string.Join"/> as extension. It concatenates the members of the collection using the specified <paramref name="separator"/> between each member.
    /// <paramref name="selector"/> is used to convert <typeparamref name="T"/> to <see cref="int"/> for concatenation.
    /// </summary>
    public static string JoinStr<T>(this IEnumerable<T> enumerable, string separator, Func<T, int> selector) =>
        string.Join(separator, enumerable.Select(x => selector(x)));

    /// <summary>
    /// This is <see cref="string.Join"/> as extension. It concatenates the members of the collection using the specified <paramref name="separator"/> between each member.
    /// </summary>
    public static string JoinStr(this IEnumerable<string> enumerable, string separator) =>
        JoinStr(enumerable, separator, x => x);

    /// <summary>
    /// This is <see cref="string.Join"/> as extension. It concatenates the members of the collection using the specified <paramref name="separator"/> between each member.
    /// </summary>
    public static string JoinStr(this IEnumerable<int> enumerable, string separator) =>
        JoinStr(enumerable, separator, x => x);

    /// <summary>
    /// Concatenates the members of a <see cref="string"/> collection using the specified <paramref name="separator"/> between each member.
    /// Any whitespace or null member of the collection will be ignored.
    /// </summary>
    public static string JoinNonEmpty(this IEnumerable<string> enumerable, string separator) =>
        string.Join(separator, enumerable.Where(x => !string.IsNullOrWhiteSpace(x)));

    /// <summary>
    /// Concatenates the members of <paramref name="values"/> using "and" as the last separator and using a
    /// <see href="https://en.wikipedia.org/wiki/Serial_comma">serial comma</see>.
    /// <list type="table">
    /// <item><c>[a, b, c] => "a, b, and c"</c></item>
    /// <item><c>[a, b] => "a and b"</c></item>
    /// <item><c>[a] => "a"</c></item>
    /// <item><c>[] or null => ""</c></item>
    /// </list>
    /// </summary>
    public static string JoinAnd<T>(this IEnumerable<T> values) =>
        values?.Select(x => x?.ToString()).Where(x => !string.IsNullOrWhiteSpace(x)).ToList() switch
        {
            { Count: > 2 } serial => $"{serial.Take(serial.Count - 1).JoinStr(", ")}, and {serial.Last()}",
            { Count: 2 } pair => $"{pair[0]} and {pair[1]}",
            { Count: 1 } single => single[0],
            _ => string.Empty,
        };

    public static IEnumerable<SecondaryLocation> ToSecondary(this IEnumerable<Location> locations, string message = null, params string[] messageArgs) =>
        locations.Select(x => x.ToSecondary(message, messageArgs));

    public static IEnumerable<SecondaryLocation> ToSecondaryLocations(this IEnumerable<SyntaxNode> nodes, string message = null, params string[] messageArgs) =>
        nodes.Select(x => x.ToSecondaryLocation(message, messageArgs));

    public static IEnumerable<SecondaryLocation> ToSecondaryLocations(this IEnumerable<SyntaxToken> nodes, string message = null, params string[] messageArgs) =>
        nodes.Select(x => x.ToSecondaryLocation(message, messageArgs));
}
