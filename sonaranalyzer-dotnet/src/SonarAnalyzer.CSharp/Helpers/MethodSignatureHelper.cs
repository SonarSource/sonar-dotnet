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

using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SonarAnalyzer.Helpers
{
    public static class MethodSignatureHelper
    {
        public static Func<MethodSignature, bool> IsSame<TSymbolType>(SimpleNameSyntax identifierName, SemanticModel semanticModel)
            where TSymbolType : class, ISymbol
        {
            var identifierText = identifierName.Identifier.ValueText;
            var identifierSymbol = new Lazy<TSymbolType>(() => semanticModel.GetSymbolInfo(identifierName).Symbol as TSymbolType);
            // This function will be called multiple times for each tracked MethodSignature,
            // hence we cache as much as possible before.
            return (methodSignature) =>
                identifierText == methodSignature.Name &&
                identifierSymbol.Value != null &&
                identifierSymbol.Value.ContainingType.Is(methodSignature.ContainingType);
        }
    }
}
