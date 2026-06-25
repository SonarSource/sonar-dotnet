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

namespace SonarAnalyzer.Core.Extensions;

public static class DictionaryExtensions
{
    extension<TKey, TValue>(IDictionary<TKey, TValue> dictionary)
    {
        public TValue GetValueOrDefault(TKey key) =>
            dictionary.GetValueOrDefault(key, default);

        public TValue GetValueOrDefault(TKey key, TValue defaultValue) =>
            dictionary.TryGetValue(key, out var result) ? result : defaultValue;

        public TValue GetOrAdd(TKey key, Func<TKey, TValue> factory)
        {
            if (!dictionary.TryGetValue(key, out var value))
            {
                value = factory(key);
                dictionary.Add(key, value);
            }
            return value;
        }

        public bool DictionaryEquals(IDictionary<TKey, TValue> other) =>
            dictionary == other
            || (EqualityComparer<TValue>.Default is var valueComparer
                && dictionary is not null
                && other is not null
                && dictionary.Count == other.Count
                && dictionary.All(x => other.TryGetValue(x.Key, out var value2) && valueComparer.Equals(x.Value, value2)));
    }
}
