/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2020 SonarSource SA
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
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SonarAnalyzer.Helpers
{
    internal static class ArgumentSyntaxExtensions
    {
        internal static IEnumerable<ArgumentSyntax> GetArgumentsOfKnownType(this SeparatedSyntaxList<ArgumentSyntax> syntaxList, KnownType knownType, SemanticModel semanticModel) =>
            syntaxList
                .Where(argument => semanticModel.GetTypeInfo(argument.Expression).Type.Is(knownType));

        internal static IEnumerable<ISymbol> GetSymbolsOfKnownType(this SeparatedSyntaxList<ArgumentSyntax> syntaxList, KnownType knownType, SemanticModel semanticModel) =>
            syntaxList
                .GetArgumentsOfKnownType(knownType, semanticModel)
                .Select(argument => semanticModel.GetSymbolInfo(argument.Expression).Symbol);
    }
}
