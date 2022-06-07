/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace SonarAnalyzer.Helpers
{
    internal abstract partial class KnownType
    {
        public string TypeName { get; private init; }

        internal abstract bool Matches(ITypeSymbol symbol);

        internal sealed class RegularKnownType : KnownType
        {
            private readonly IList<string> namespaceParts;
            private readonly string[] genericParameters;

            public bool IsArray { get; init; }

            internal RegularKnownType(string fullTypeName, params string[] genericParameters)
            {
                this.genericParameters = genericParameters ?? Array.Empty<string>();

                var parts = fullTypeName.Split('.');
                TypeName = parts[parts.Length - 1];
                namespaceParts = new ArraySegment<string>(parts, 0, parts.Length - 1);
            }

            internal override bool Matches(ITypeSymbol symbol) =>
                IsMatch(symbol)
                || IsMatch(symbol.OriginalDefinition);

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
                    : !genericParameters.Any();

            private bool NamespaceMatches(ISymbol symbol)
            {
                // For performance reason we want to avoid building full namespace as a string to compare it with the expected value.
                var currentNamespace = symbol.ContainingNamespace;
                var index = namespaceParts.Count - 1;

                while (currentNamespace != null && !string.IsNullOrEmpty(currentNamespace.Name) && index >= 0)
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

        internal sealed class SpecialKnownType : KnownType
        {
            private readonly SpecialType specialType;

            public SpecialKnownType(SpecialType specialType, string typeName)
            {
                this.specialType = specialType;
                TypeName = typeName;
            }

            internal override bool Matches(ITypeSymbol symbol) =>
                symbol.SpecialType == specialType
                || symbol.OriginalDefinition.SpecialType == specialType;
        }
    }
}
