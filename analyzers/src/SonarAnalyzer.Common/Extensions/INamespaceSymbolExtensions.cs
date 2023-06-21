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
    public static bool Is(this INamespaceSymbol symbol, string name)
    {
        var ns = name.Split(new[] { ".", "::" }, StringSplitOptions.RemoveEmptyEntries);
        for (var i = ns.Length - 1; i >= 0; i--)
        {
            if (symbol is null || symbol.Name != ns[i])
            {
                return i == 0 && ns[i] == "global" && symbol?.IsGlobalNamespace is true;
            }
            else
            {
                symbol = symbol.ContainingNamespace;
            }
        }
        return symbol is null || symbol.IsGlobalNamespace;
    }
}
