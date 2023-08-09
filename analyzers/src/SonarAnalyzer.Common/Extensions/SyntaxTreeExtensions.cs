/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace SonarAnalyzer.Extensions;

internal static class SyntaxTreeExtensions
{
    private static readonly ConditionalWeakTable<Compilation, ConcurrentDictionary<SyntaxTree, bool>> GeneratedCodeCache = new();

    public static bool IsGenerated(this SyntaxTree tree, GeneratedCodeRecognizer generatedCodeRecognizer, Compilation compilation)
    {
        if (tree == null)
        {
            return false;
        }
        var cache = GeneratedCodeCache.GetOrCreateValue(compilation);
        // Hotpath: Don't use cache.GetOrAdd that takes a factory method. It allocates a delegate which causes GC preasure.
        return cache.TryGetValue(tree, out var isGenerated)
            ? isGenerated
            : cache.GetOrAdd(tree, generatedCodeRecognizer.IsConsideredGenerated(tree));
    }
}
