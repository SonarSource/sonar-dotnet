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

        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (var element in enumerable)
            {
                action(element);
            }
        }

        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T> enumerable)
            where T : class
        {
            return enumerable.Where(e => e != null);
        }
    }
}
