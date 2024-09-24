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

using System.Text;

namespace SonarAnalyzer.Core.Semantics;

[DebuggerDisplay("{DebuggerDisplay}")]
public sealed partial class KnownType
{
    private readonly IList<string> namespaceParts;
    private readonly string[] genericParameters;

    public string TypeName { get; }
    public string FullName { get; }
    public bool IsArray { get; init; }
    public IReadOnlyList<string> GenericParameters => genericParameters;
    public string MetadataName => $"{FullName}{(GenericParameters.Any() ? $"`{GenericParameters.Count}" : string.Empty)}";

    internal string DebuggerDisplay
    {
        get
        {
            var sb = new StringBuilder(FullName);
            if (genericParameters.Length > 0)
            {
                sb.Append('<').Append(genericParameters.JoinStr(", ")).Append('>');
            }
            if (IsArray)
            {
                sb.Append("[]");
            }
            return sb.ToString();
        }
    }

    public KnownType(string fullName, params string[] genericParameters)
    {
        var parts = fullName.Split('.');
        namespaceParts = new ArraySegment<string>(parts, 0, parts.Length - 1);
        this.genericParameters = genericParameters;
        FullName = fullName;
        TypeName = parts[parts.Length - 1];
    }

    public bool Matches(ITypeSymbol symbol) =>
        IsMatch(symbol) || IsMatch(symbol.OriginalDefinition);

    private bool IsMatch(ITypeSymbol symbol)
    {
        _ = symbol ?? throw new ArgumentNullException(nameof(symbol));
        if (IsArray)
        {
            if (symbol is IArrayTypeSymbol array)
            {
                symbol = array.ElementType;
            }
            else
            {
                return false;
            }
        }

        return symbol.Name == TypeName
            && NamespaceMatches(symbol)
            && GenericParametersMatch(symbol);
    }

    private bool GenericParametersMatch(ISymbol symbol) =>
        symbol is INamedTypeSymbol namedType
            ? namedType.TypeParameters.Select(x => x.Name).SequenceEqual(genericParameters)
            : genericParameters.Length == 0;

    private bool NamespaceMatches(ISymbol symbol)
    {
        var currentNamespace = symbol.ContainingNamespace;
        var index = namespaceParts.Count - 1;
        while (currentNamespace is not null && !string.IsNullOrEmpty(currentNamespace.Name) && index >= 0)
        {
            if (currentNamespace.Name != namespaceParts[index])
            {
                return false;
            }

            currentNamespace = currentNamespace.ContainingNamespace;
            index--;
        }
        return index == -1 && string.IsNullOrEmpty(currentNamespace?.Name);
    }
}
