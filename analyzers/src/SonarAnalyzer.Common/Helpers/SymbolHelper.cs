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

namespace SonarAnalyzer.Helpers
{
    public static class SymbolHelper
    {

        public static bool IsExtensionOn(this IMethodSymbol methodSymbol, KnownType type)
        {
            if (!(methodSymbol is { IsExtensionMethod: true }))
            {
                return false;
            }

            var receiverType = methodSymbol.ReceiverType as INamedTypeSymbol;

            if (methodSymbol.MethodKind == MethodKind.Ordinary)
            {
                receiverType = methodSymbol.Parameters.First().Type as INamedTypeSymbol;
            }

            var constructedFrom = receiverType?.ConstructedFrom;
            return constructedFrom.Is(type);
        }

        public static bool IsAnyAttributeInOverridingChain(IPropertySymbol propertySymbol) =>
            IsAnyAttributeInOverridingChain(propertySymbol, property => property.OverriddenProperty);

        public static bool IsAnyAttributeInOverridingChain(IMethodSymbol methodSymbol) =>
            IsAnyAttributeInOverridingChain(methodSymbol, method => method.OverriddenMethod);


        public static bool IsDestructor(this IMethodSymbol method) =>
            method.MethodKind == MethodKind.Destructor;


        internal static bool IsKnownType(this SyntaxNode syntaxNode, KnownType knownType, SemanticModel semanticModel)
        {
            var symbolType = semanticModel.GetSymbolInfo(syntaxNode).Symbol.GetSymbolType();

            return symbolType.Is(knownType) || symbolType?.OriginalDefinition?.Is(knownType) == true;
        }

        internal static bool IsDeclarationKnownType(this SyntaxNode syntaxNode, KnownType knownType, SemanticModel semanticModel)
        {
            var symbolType = semanticModel.GetDeclaredSymbol(syntaxNode)?.GetSymbolType();
            return symbolType.Is(knownType);
        }

        private static bool IsAnyAttributeInOverridingChain<TSymbol>(TSymbol symbol, Func<TSymbol, TSymbol> getOverriddenMember)
            where TSymbol : class, ISymbol
        {
            var currentSymbol = symbol;
            while (currentSymbol != null)
            {
                if (currentSymbol.GetAttributes().Any())
                {
                    return true;
                }

                if (!currentSymbol.IsOverride)
                {
                    return false;
                }

                currentSymbol = getOverriddenMember(currentSymbol);
            }

            return false;
        }

    }
}
