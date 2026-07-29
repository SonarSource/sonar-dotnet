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

namespace SonarAnalyzer.Core.Semantics.Extensions;

internal static class INamespaceSymbolExtensions
{
    extension(INamespaceSymbol symbol)
    {
        /// <summary>
        /// Checks if the <see cref="INamespaceSymbol"/> fits the <paramref name="name"/>. The format of <paramref name="name"/> is the same as in a <see langword="using"/> directive.
        /// </summary>
        /// <param name="name">The name in the form <c>System.Collections.Generic</c>.</param>
        /// <returns>Returns <see langword="true"/> if the namespace symbol refers to the string given.</returns>
        public bool Is(string name)
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

        public IEnumerable<INamedTypeSymbol> AllNamedTypes
        {
            get
            {
                if (symbol is null)
                {
                    yield break;
                }
                foreach (var typeMember in symbol.GetTypeMembers().SelectMany(x => x.AllNamedTypes))
                {
                    yield return typeMember;
                }
                foreach (var typeMember in symbol.GetNamespaceMembers().SelectMany(x => x.AllNamedTypes))
                {
                    yield return typeMember;
                }
            }
        }

        public bool IsSameNamespace(INamespaceSymbol namespace2) =>
            (symbol.IsGlobalNamespace && namespace2.IsGlobalNamespace)
            || (symbol.Name.Equals(namespace2.Name)
                && symbol.ContainingNamespace is not null
                && namespace2.ContainingNamespace is not null
                && symbol.ContainingNamespace.IsSameNamespace(namespace2.ContainingNamespace));

        public bool IsSameOrAncestorOf(INamespaceSymbol namespaceToCheck) =>
            symbol.IsSameNamespace(namespaceToCheck)
            || (namespaceToCheck.ContainingNamespace is not null && symbol.IsSameOrAncestorOf(namespaceToCheck.ContainingNamespace));
    }
}
