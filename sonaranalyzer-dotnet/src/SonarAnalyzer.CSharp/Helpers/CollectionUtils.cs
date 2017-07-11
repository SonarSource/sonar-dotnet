/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2017 SonarSource SA
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
using System.Linq;

namespace SonarAnalyzer.Helpers
{
    public static class CollectionUtils
    {
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable == null)
            {
                return new HashSet<T>();
            }

            return new HashSet<T>(enumerable);
        }

        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            return dictionary.GetValueOrDefault(key, default(TValue));
        }

        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
        {
            TValue result;
            if (dictionary.TryGetValue(key, out result))
            {
                return result;
            }

            return defaultValue;
        }

        /// <summary>
        /// Compares each element between two collections. The elements needs in the same order to be considered equal.
        /// </summary>
        public static bool AreEqual<T, V>(IEnumerable<T> collection1, IEnumerable<V> collection2,
            Func<T, V, bool> comparator)
        {
            var enum1 = collection1.GetEnumerator();
            var enum2 = collection2.GetEnumerator();

            bool enum1HasNext = enum1.MoveNext();
            bool enum2HasNext = enum2.MoveNext();

            while (enum1HasNext && enum2HasNext)
            {
                if (!comparator(enum1.Current, enum2.Current))
                {
                    return false;
                }

                enum1HasNext = enum1.MoveNext();
                enum2HasNext = enum2.MoveNext();
            }

            return enum1HasNext == enum2HasNext;
        }

        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T> enumerable)
            where T : class
        {
            return enumerable.Where(e => e != null);
        }

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
        public static IEnumerable<TResult> Merge<TFirst, TSecond, TResult>(this IEnumerable<TFirst> first,
            IEnumerable<TSecond> second, Func<TFirst, TSecond, TResult> operation)
        {
            using (var iter1 = first.GetEnumerator())
            using (var iter2 = second.GetEnumerator())
            {
                while (iter1.MoveNext())
                {
                    yield return operation(iter1.Current,
                         iter2.MoveNext() ? iter2.Current : default(TSecond));
                }

                while (iter2.MoveNext())
                {
                    yield return operation(default(TFirst), iter2.Current);
                }
            }
        }
    }
}
