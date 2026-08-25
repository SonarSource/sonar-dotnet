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

using Roslyn.Utilities;

namespace SonarAnalyzer.ShimLayer.Extensions;

internal static class TypeExtensions
{
    extension(Type type)
    {
        [PerformanceSensitive("https://github.com/SonarSource/sonar-dotnet/issues/8106", AllowCaptures = false, AllowGenericEnumeration = false, AllowImplicitBoxing = false)] // Sonar
        public bool CanWrap(ConcurrentDictionary<Type, bool> cache, object instance)
        {
            if (instance is null || type is null)
            {
                return false;
            }
            var instanceType = instance.GetType();
            if (!cache.TryGetValue(instanceType, out var result))
            {
                result = type.IsAssignableFrom(instanceType);
                cache.TryAdd(instanceType, result);
            }
            return result;
        }
    }
}
