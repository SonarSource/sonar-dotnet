/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace SonarAnalyzer.Security.Ucfg
{
    public static class MethodIdProvider
    {
        public static string Create(ISymbol symbol)
        {
            if (symbol == null)
            {
                return KnownMethodId.Unknown;
            }

            switch (symbol.Kind)
            {
                case SymbolKind.Method:
                    var methodSymbol = (IMethodSymbol)symbol;
                    var method = methodSymbol.ReducedFrom ?? methodSymbol.OriginalDefinition;
                    return $"{Create(method.ContainingType)}.{method.Name}{TypeArguments(method)}({Join(method.Parameters.Select(Create))})";

                case SymbolKind.Parameter:
                    var p = (IParameterSymbol)symbol;
                    return $"{Create(p.Type)}";

                case SymbolKind.NamedType:
                    var namedType = (INamedTypeSymbol)symbol;
                    return $"{namedType.ContainingAssembly.Name};{namedType.ToDisplayString()}";

                case SymbolKind.TypeParameter:
                    return symbol.Name;

                case SymbolKind.Property:
                    var property = (IPropertySymbol)symbol;
                    return Create(property.GetMethod);

                default:
                    return KnownMethodId.Unknown;
            }
        }

        private static string Join(IEnumerable<string> strings) =>
            string.Join(",", strings);

        private static string TypeArguments(IMethodSymbol methodSymbol) =>
            methodSymbol.TypeArguments.Length == 0
                ? string.Empty
                : $"<{Join(methodSymbol.TypeArguments.Select(GetName))}>";

        private static string GetName(ITypeSymbol typeSymbol) =>
            typeSymbol.Name;
    }
}
