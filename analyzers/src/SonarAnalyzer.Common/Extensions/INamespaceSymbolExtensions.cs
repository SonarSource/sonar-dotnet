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


namespace SonarAnalyzer.Extensions;

internal static class INamespaceSymbolExtensions
{
    /// <summary>
    /// Checks if the <see cref="INamespaceSymbol"/> fits the <paramref name="name"/>. The format of <paramref name="name"/> is the same as in a <see langword="using"/> directive.
    /// </summary>
    /// <param name="symbol">The namespace symbol to test.</param>
    /// <param name="name">The name in the form <c>System.Collections.Generic</c>.</param>
    /// <returns>Returns <see langword="true"/> if the namespace symbol refers to the string given.</returns>
    public static bool Is(this INamespaceSymbol? symbol, string name)
    {
        if (name is null)
        {
            throw new ArgumentNullException(nameof(name));
        }
        if (symbol is null)
        {
            return false;
        }

        var ns = name.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
        for (var i = ns.Length - 1; i >= 0; i--)
        {
            if (symbol is null || symbol.Name != ns[i])
            {
                return false;
            }
            else
            {
                symbol = symbol.ContainingNamespace;
            }
        }
        return symbol?.IsGlobalNamespace is true;
    }
}
