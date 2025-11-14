/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.Core.Semantics.Extensions;

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
            && namespace1.ContainingNamespace.IsSameNamespace(namespace2.ContainingNamespace));

    public static bool IsSameOrAncestorOf(this INamespaceSymbol thisNamespace, INamespaceSymbol namespaceToCheck) =>
        thisNamespace.IsSameNamespace(namespaceToCheck)
        || (namespaceToCheck.ContainingNamespace is not null && thisNamespace.IsSameOrAncestorOf(namespaceToCheck.ContainingNamespace));
}
