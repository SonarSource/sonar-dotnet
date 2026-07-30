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

using System.Collections;
using System.Reflection;

namespace SonarAnalyzer.CFG.Extensions;

internal static class PropertyInfoExtensions
{
    extension(PropertyInfo property)
    {
        public T ReadCached<T>(object instance, ref T cache) where T : class =>
            cache ??= (T)property.GetValue(instance);

        public T ReadCached<T>(object instance, ref T? cache) where T : struct =>
            cache ??= (T)property.GetValue(instance);

        public T ReadCached<T>(object instance, Func<object, T> createInstance, ref T cache) where T : class =>
            cache ??= createInstance(property.GetValue(instance));

        public ImmutableArray<T> ReadCached<T>(object instance, ref ImmutableArray<T> cache) =>
            property.ReadCached(instance, x => (T)x, ref cache);

        public ImmutableArray<T> ReadCached<T>(object instance, Func<object, T> createInstance, ref ImmutableArray<T> cache)
        {
            if (cache.IsDefault)
            {
                cache = ((IEnumerable)property.GetValue(instance)).Cast<object>().Select(createInstance).ToImmutableArray();
            }
            return cache;
        }
    }
}
