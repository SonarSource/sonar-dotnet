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

using System.Collections;
using System.Reflection;

namespace SonarAnalyzer.CFG
{
    internal static class PropertyInfoExtensions
    {
        public static T ReadCached<T>(this PropertyInfo property, object instance, ref T cache) where T : class =>
            cache ??= (T)property.GetValue(instance);

        public static T ReadCached<T>(this PropertyInfo property, object instance, ref T? cache) where T : struct =>
            cache ??= (T)property.GetValue(instance);

        public static T ReadCached<T>(this PropertyInfo property, object instance, Func<object, T> createInstance, ref T cache) where T : class =>
            cache ??= createInstance(property.GetValue(instance));

        public static ImmutableArray<T> ReadCached<T>(this PropertyInfo property, object instance, ref ImmutableArray<T> cache) =>
            ReadCached(property, instance, x => (T)x, ref cache);

        public static ImmutableArray<T> ReadCached<T>(this PropertyInfo property, object instance, Func<object, T> createInstance, ref ImmutableArray<T> cache)
        {
            if (cache.IsDefault)
            {
                cache = ((IEnumerable)property.GetValue(instance)).Cast<object>().Select(createInstance).ToImmutableArray();
            }
            return cache;
        }
    }
}
