/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using System.Text;

namespace SonarAnalyzer.Core.Extensions;

public static class IEnumerableExtensions
{
    extension<T>(IEnumerable<T> enumerable)
    {
        public HashSet<T> ToHashSet(IEqualityComparer<T> equalityComparer = null)
        {
            enumerable ??= [];
            return equalityComparer is null
                ? new(enumerable)
                : new(enumerable, equalityComparer);
        }

        public bool IsEmpty => !enumerable.Any();
    }

    /// <summary>
    /// Compares each element between two collections. The elements needs in the same order to be considered equal.
    /// </summary>
    extension<T>(IEnumerable<T> first)
    {
        public bool Equals<V>(IEnumerable<V> second, Func<T, V, bool> predicate)
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

        /// <summary>
        /// Applies a specified function to the corresponding elements of two sequences,
        /// producing a sequence of the results. If the collections have different length
        /// default(T) will be passed in the operation function for the corresponding items that
        /// do not exist.
        /// </summary>
        /// <typeparam name="TSecond">The type of the elements of the second input sequence.</typeparam>
        /// <typeparam name="TResult">The type of the elements of the result sequence.</typeparam>
        /// <param name="second">The second sequence to merge.</param>
        /// <param name="operation">A function that specifies how to merge the elements from the two sequences.</param>
        /// <returns>An System.Collections.Generic.IEnumerable`1 that contains merged elements of
        /// two input sequences.</returns>
        public IEnumerable<TResult> Merge<TSecond, TResult>(IEnumerable<TSecond> second, Func<T, TSecond, TResult> operation)
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
    }

    extension<T>(IEnumerable<T> enumerable) where T : class
    {
        public IEnumerable<T> WhereNotNull() =>
            enumerable.Where(x => x is not null);
    }

    extension<T>(IEnumerable<T?> enumerable) where T : struct
    {
        public IEnumerable<T> WhereNotNull() =>
            enumerable.Where(x => x.HasValue).Select(x => x.Value);
    }

    extension<T>(IEnumerable<T> enumerable)
    {
        public int IndexOf(Func<T, bool> predicate) =>
            enumerable.Select((item, index) => new { item, index }).FirstOrDefault(x => predicate(x.item))?.index ?? -1;

        /// <summary>
        /// This is <see cref="string.Join"/> as extension. It concatenates the members of the collection using the specified <paramref name="separator"/> between each member.
        /// <paramref name="selector"/> is used to convert <typeparamref name="T"/> to <see cref="string"/> for concatenation.
        /// </summary>
        public string JoinStr(string separator, Func<T, string> selector) =>
            string.Join(separator, enumerable.Select(x => selector(x)));

        /// <summary>
        /// This is <see cref="string.Join"/> as extension. It concatenates the members of the collection using the specified <paramref name="separator"/> between each member.
        /// <paramref name="selector"/> is used to convert <typeparamref name="T"/> to <see cref="int"/> for concatenation.
        /// </summary>
        public string JoinStr(string separator, Func<T, int> selector) =>
            string.Join(separator, enumerable.Select(x => selector(x)));

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
        public string JoinAnd() =>
            enumerable.JoinWith("and");

        /// <summary>
        /// Concatenates the members of <paramref name="values"/> using "or" as the last separator and using a
        /// <see href="https://en.wikipedia.org/wiki/Serial_comma">serial comma</see>.
        /// <list type="table">
        /// <item><c>[a, b, c] => "a, b, or c"</c></item>
        /// <item><c>[a, b] => "a or b"</c></item>
        /// <item><c>[a] => "a"</c></item>
        /// <item><c>[] or null => ""</c></item>
        /// </list>
        /// </summary>
        public string JoinOr() =>
            enumerable.JoinWith("or");

        private string JoinWith(string with) =>
            enumerable?.Select(x => x?.ToString()).Where(x => !string.IsNullOrWhiteSpace(x)).ToList() switch
            {
                { Count: > 2 } serial => $"{serial.Take(serial.Count - 1).JoinStr(", ")}, {with} {serial[serial.Count - 1]}",
                { Count: 2 } pair => $"{pair[0]} {with} {pair[1]}",
                { Count: 1 } single => single[0],
                _ => string.Empty,
            };
    }

    extension(IEnumerable<string> enumerable)
    {
        /// <summary>
        /// This is <see cref="string.Join"/> as extension. It concatenates the members of the collection using the specified <paramref name="separator"/> between each member.
        /// </summary>
        public string JoinStr(string separator) =>
            JoinStr(enumerable, separator, x => x);

        /// <summary>
        /// Concatenates the members of a <see cref="string"/> collection using the specified <paramref name="separator"/> between each member.
        /// Any whitespace or null member of the collection will be ignored.
        /// </summary>
        public string JoinNonEmpty(string separator) =>
            string.Join(separator, enumerable.Where(x => !string.IsNullOrWhiteSpace(x)));

        public string ToSentence(bool quoteWords = false)
        {
            var wordCollection = enumerable as ICollection<string> ?? enumerable.ToList();
            var singleQuoteOrBlank = quoteWords ? "'" : string.Empty;

            return wordCollection.Count switch
            {
                0 => null,
                1 => string.Concat(singleQuoteOrBlank, wordCollection.First(), singleQuoteOrBlank),
                _ => new StringBuilder(singleQuoteOrBlank)
                        .Append(string.Join($"{singleQuoteOrBlank}, {singleQuoteOrBlank}", wordCollection.Take(wordCollection.Count - 1)))
                        .Append(singleQuoteOrBlank)
                        .Append(" and ")
                        .Append(singleQuoteOrBlank)
                        .Append(wordCollection.Last())
                        .Append(singleQuoteOrBlank)
                        .ToString(),
            };
        }
    }

    extension(IEnumerable<int> enumerable)
    {
        /// <summary>
        /// This is <see cref="string.Join"/> as extension. It concatenates the members of the collection using the specified <paramref name="separator"/> between each member.
        /// </summary>
        public string JoinStr(string separator) =>
            JoinStr(enumerable, separator, x => x);
    }

    extension(IEnumerable<Location> locations)
    {
        public IEnumerable<SecondaryLocation> ToSecondary(string message = null, params string[] messageArgs) =>
            locations.Select(x => x.ToSecondary(message, messageArgs));
    }

    extension(IEnumerable<SyntaxNode> nodes)
    {
        public IEnumerable<SecondaryLocation> ToSecondaryLocations(string message = null, params string[] messageArgs) =>
            nodes.Select(x => x.ToSecondaryLocation(message, messageArgs));
    }

    extension(IEnumerable<SyntaxToken> nodes)
    {
        public IEnumerable<SecondaryLocation> ToSecondaryLocations(string message = null, params string[] messageArgs) =>
            nodes.Select(x => x.ToSecondaryLocation(message, messageArgs));
    }
}
