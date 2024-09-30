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

namespace SonarAnalyzer.Core.Extensions;

internal static class INamespaceSymbolExtensions
{
    /// <summary>
    /// Checks if the <see cref="INamespaceSymbol"/> fits the <paramref name="name"/>. The format of <paramref name="name"/> is the same as in a <see langword="using"/> directive.
    /// </summary>
    /// <param name="symbol">The namespace symbol to test.</param>
    /// <param name="name">The name in the form <c>System.Collections.Generic</c>.</param>
    /// <returns>Returns <see langword="true"/> if the namespace symbol refers to the string given.</returns>
    public static bool Is(this INamespaceSymbol symbol, string name)
    {
        _ = name ?? throw new ArgumentNullException(nameof(name));
        var ns = name.Split(['.'], StringSplitOptions.RemoveEmptyEntries);
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

    public static IEnumerable<INamedTypeSymbol> GetAllNamedTypes(this INamespaceSymbol @namespace)
    {
        if (@namespace is null)
        {
            yield break;
        }
        foreach (var typeMember in @namespace.GetTypeMembers().SelectMany(x => x.GetAllNamedTypes()))
        {
            yield return typeMember;
        }
        foreach (var typeMember in @namespace.GetNamespaceMembers().SelectMany(GetAllNamedTypes))
        {
            yield return typeMember;
        }
    }

    public static bool IsSameNamespace(this INamespaceSymbol namespace1, INamespaceSymbol namespace2) =>
        (namespace1.IsGlobalNamespace && namespace2.IsGlobalNamespace)
        || (namespace1.Name.Equals(namespace2.Name)
            && namespace1.ContainingNamespace is not null
            && namespace2.ContainingNamespace is not null
            && IsSameNamespace(namespace1.ContainingNamespace, namespace2.ContainingNamespace));

    public static bool IsSameOrAncestorOf(this INamespaceSymbol thisNamespace, INamespaceSymbol namespaceToCheck) =>
        IsSameNamespace(thisNamespace, namespaceToCheck)
        || (namespaceToCheck.ContainingNamespace is not null && IsSameOrAncestorOf(thisNamespace, namespaceToCheck.ContainingNamespace));
}
