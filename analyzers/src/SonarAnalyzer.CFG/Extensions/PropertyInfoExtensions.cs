/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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

using System.Collections;
using System.Reflection;

namespace SonarAnalyzer.CFG.Extensions;

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
