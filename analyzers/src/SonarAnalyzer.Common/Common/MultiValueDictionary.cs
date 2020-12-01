/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2020 SonarSource SA
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
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace SonarAnalyzer.Common
{
    [Serializable]
    public class MultiValueDictionary<TKey, TValue> : Dictionary<TKey, ICollection<TValue>>
    {
        public MultiValueDictionary()
        {
        }

        [ExcludeFromCodeCoverage]
        protected MultiValueDictionary(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public static MultiValueDictionary<TKey, TValue> Create<TUnderlying>()
            where TUnderlying : ICollection<TValue>, new()
        {
            return new MultiValueDictionary<TKey, TValue>
            {
                UnderlyingCollectionFactory = () => new TUnderlying()
            };
        }

        private Func<ICollection<TValue>> UnderlyingCollectionFactory { get; set; } = () => new List<TValue>();

        public void Add(TKey key, TValue value)
        {
            AddWithKey(key, value);
        }

        public void AddWithKey(TKey key, TValue value)
        {
            if (!TryGetValue(key, out var values))
            {
                values = UnderlyingCollectionFactory();
                Add(key, values);
            }
            values.Add(value);
        }

        public void AddRangeWithKey(TKey key, IEnumerable<TValue> addedValues)
        {
            if (!TryGetValue(key, out var values))
            {
                values = UnderlyingCollectionFactory();
                Add(key, values);
            }
            foreach (var addedValue in addedValues)
            {
                values.Add(addedValue);
            }
        }

        [ExcludeFromCodeCoverage]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }

    #region Extensions

    public static class MultiValueDictionaryExtensions
    {
        public static MultiValueDictionary<TKey, TElement> ToMultiValueDictionary<TSource, TKey, TElement>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, ICollection<TElement>> elementSelector)
            where TSource : Tuple<TKey, ICollection<TElement>>
        {
            var dictionary = new MultiValueDictionary<TKey, TElement>();
            foreach (var item in source)
            {
                dictionary.Add(keySelector(item), elementSelector(item));
            }
            return dictionary;
        }
    }

    #endregion Extensions
}
