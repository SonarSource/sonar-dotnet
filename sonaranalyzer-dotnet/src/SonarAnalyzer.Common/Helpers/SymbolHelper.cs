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
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace SonarAnalyzer.Helpers
{
    internal static class SymbolHelper
    {
        public static IEnumerable<INamedTypeSymbol> GetAllNamedTypes(this INamespaceSymbol @namespace)
        {
            if (@namespace == null)
            {
                yield break;
            }

            foreach (var typeMember in @namespace.GetTypeMembers().SelectMany(t => GetAllNamedTypes(t)))
            {
                yield return typeMember;
            }

            foreach (var typeMember in @namespace.GetNamespaceMembers().SelectMany(t => GetAllNamedTypes(t)))
            {
                yield return typeMember;
            }
        }

        public static bool IsObsolete(this ISymbol symbol)
        {
            return symbol != null &&
                symbol.GetAttributes().Any(a => a.AttributeClass.Is(KnownType.System_ObsoleteAttribute));
        }

        public static IEnumerable<INamedTypeSymbol> GetAllNamedTypes(this INamedTypeSymbol type)
        {
            if (type == null)
            {
                yield break;
            }

            yield return type;

            foreach (var nestedType in type.GetTypeMembers().SelectMany(t => GetAllNamedTypes(t)))
            {
                yield return nestedType;
            }
        }

        public static T GetInterfaceMember<T>(this T symbol)
            where T : class, ISymbol
        {
            if (!CanSymbolBeInterfaceMemberOrOverride(symbol))
            {
                return null;
            }

            if (symbol.IsOverride)
            {
                return null;
            }

            return symbol.ContainingType
                .AllInterfaces
                .SelectMany(@interface => @interface.GetMembers())
                .OfType<T>()
                .FirstOrDefault(member => symbol.Equals(symbol.ContainingType.FindImplementationForInterfaceMember(member)));
        }

        public static T GetOverriddenMember<T>(this T symbol)
            where T : class, ISymbol
        {
            if (!CanSymbolBeInterfaceMemberOrOverride(symbol))
            {
                return null;
            }

            if (!symbol.IsOverride)
            {
                return null;
            }

            switch (symbol.Kind)
            {
                case SymbolKind.Method:
                    return (T)((IMethodSymbol)symbol).OverriddenMethod;

                case SymbolKind.Property:
                    return (T)((IPropertySymbol)symbol).OverriddenProperty;

                case SymbolKind.Event:
                    return (T)((IEventSymbol)symbol).OverriddenEvent;

                default:
                    throw new ArgumentException(
                        $"Only methods, properties and events can be overridden. {typeof(T).Name} was provided",
                        nameof(symbol));
            }
        }

        public static bool CanSymbolBeInterfaceMemberOrOverride(ISymbol symbol)
        {
            return symbol is IMethodSymbol ||
                symbol is IPropertySymbol ||
                symbol is IEventSymbol;
        }

        public static IEnumerable<INamedTypeSymbol> GetSelfAndBaseTypes(this INamedTypeSymbol type)
        {
            if (type == null)
            {
                yield break;
            }

            var baseType = type;
            while (baseType != null &&
                !(baseType is IErrorTypeSymbol))
            {
                yield return baseType;
                baseType = baseType.BaseType;
            }
        }

        public static bool IsChangeable(this ISymbol symbol)
        {
            return !symbol.IsAbstract &&
                !symbol.IsVirtual &&
                symbol.GetInterfaceMember() == null &&
                symbol.GetOverriddenMember() == null;
        }

        public static bool IsExtensionOn(this IMethodSymbol methodSymbol, KnownType type)
        {
            if (methodSymbol == null ||
                !methodSymbol.IsExtensionMethod)
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

        public static IEnumerable<IParameterSymbol> GetParameters(this ISymbol symbol)
        {
            if (symbol is IMethodSymbol methodSymbol)
            {
                return methodSymbol.Parameters;
            }

            if (symbol is IPropertySymbol propertySymbol)
            {
                return propertySymbol.Parameters;
            }

            return Enumerable.Empty<IParameterSymbol>();
        }

        public static bool IsAnyAttributeInOverridingChain(IPropertySymbol propertySymbol)
        {
            return IsAnyAttributeInOverridingChain(propertySymbol, property => property.OverriddenProperty);
        }

        public static bool IsAnyAttributeInOverridingChain(IMethodSymbol methodSymbol)
        {
            return IsAnyAttributeInOverridingChain(methodSymbol, method => method.OverriddenMethod);
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

        public static Accessibility GetEffectiveAccessibility(this ISymbol symbol)
        {
            if (symbol == null)
            {
                return Accessibility.NotApplicable;
            }

            var result = symbol.DeclaredAccessibility;
            if (result == Accessibility.Private)
            {
                return Accessibility.Private;
            }

            for (var container = symbol.ContainingType; container != null; container = container.ContainingType)
            {
                if (container.DeclaredAccessibility == Accessibility.Private)
                {
                    return Accessibility.Private;
                }

                if (container.DeclaredAccessibility == Accessibility.Internal)
                {
                    result = Accessibility.Internal;
                }
            }

            return result;
        }

        public static bool IsPubliclyAccessible(this ISymbol symbol)
        {
            var effectiveAccessibility = GetEffectiveAccessibility(symbol);

            return effectiveAccessibility == Accessibility.Public ||
                effectiveAccessibility == Accessibility.Protected ||
                effectiveAccessibility == Accessibility.ProtectedOrInternal;
        }

        public static bool IsConstructor(this ISymbol symbol)
        {
            return symbol.Kind == SymbolKind.Method && symbol.Name == ".ctor";
        }

        public static bool IsDestructor(this IMethodSymbol method)
        {
            return method.MethodKind == MethodKind.Destructor;
        }

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

        public static bool IsStatic(this ISymbol symbol) =>
            symbol != null && symbol.IsStatic;

        internal static bool IsExtensionMethod(this SyntaxNode expression, SemanticModel semanticModel) =>
            semanticModel.GetSymbolInfo(expression).Symbol is IMethodSymbol memberSymbol &&
                memberSymbol.IsExtensionMethod;
    }
}
