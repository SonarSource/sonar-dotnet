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
using Microsoft.CodeAnalysis.VisualBasic.Syntax;


    /**********************************************************
    * When maintaining this class, don't forget to change the
    * corresponding class in the C# analyzer
    *********************************************************/

namespace SonarAnalyzer.Helpers
{
    internal static class MethodSignatureHelper
    {
        public static bool IsMatch<TSymbolType>(SimpleNameSyntax identifierName, SemanticModel semanticModel,
            Lazy<TSymbolType> symbolFetcher, bool checkOverriddenMethods, MethodSignature[] methods)
            where TSymbolType : class, ISymbol
        {
            if (identifierName == null)
            {
                return false;
            }

            var identifierText = identifierName.Identifier.ValueText;
            foreach (var m in methods)
            {
                if (identifierText != m.Name)
                {
                    continue;
                }
                if (symbolFetcher.Value == null)
                {
                    return false; // No need to continue looping if the symbol is null
                }
                if (symbolFetcher.Value.ContainingType.ConstructedFrom.Is(m.ContainingType))
                {
                    return true;
                }
                if (checkOverriddenMethods && IsOverride(m))
                {
                    return true;
                }
            }

            bool IsOverride(MethodSignature signature)
            {
                var currentMethod = symbolFetcher.Value.GetOverriddenMember();

                while (currentMethod != null)
                {
                    if (currentMethod.ContainingType.ConstructedFrom.Is(signature.ContainingType))
                    {
                        return true;
                    }
                    currentMethod = currentMethod.GetOverriddenMember();
                }
                return false;
            }

            return false;
        }
    }
}
