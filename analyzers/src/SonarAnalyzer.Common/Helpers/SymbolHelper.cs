/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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
using System.Collections.Immutable;
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

            foreach (var typeMember in @namespace.GetTypeMembers().SelectMany(GetAllNamedTypes))
            {
                yield return typeMember;
            }

            foreach (var typeMember in @namespace.GetNamespaceMembers().SelectMany(GetAllNamedTypes))
            {
                yield return typeMember;
            }
        }

        public static IEnumerable<INamedTypeSymbol> GetAllNamedTypes(this INamedTypeSymbol type)
        {
            if (type == null)
            {
                yield break;
            }

            yield return type;

            foreach (var nestedType in type.GetTypeMembers().SelectMany(GetAllNamedTypes))
            {
                yield return nestedType;
            }
        }

        public static T GetInterfaceMember<T>(this T symbol)
            where T : class, ISymbol
        {
            if (symbol == null
                || symbol.IsOverride
                || !CanBeInterfaceMember(symbol))
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
            if (!(symbol is {IsOverride: true}))
            {
                return null;
            }

            return symbol.Kind switch
            {
                SymbolKind.Method => (T)((IMethodSymbol)symbol).OverriddenMethod,
                SymbolKind.Property => (T)((IPropertySymbol)symbol).OverriddenProperty,
                SymbolKind.Event => (T)((IEventSymbol)symbol).OverriddenEvent,
                _ => throw new ArgumentException($"Only methods, properties and events can be overridden. {typeof(T).Name} was provided", nameof(symbol))
            };
        }

        public static IEnumerable<INamedTypeSymbol> GetSelfAndBaseTypes(this ITypeSymbol type)
        {
            if (type == null)
            {
                yield break;
            }

            var currentType = type;
            while (currentType?.Kind == SymbolKind.NamedType)
            {
                yield return (INamedTypeSymbol)currentType;
                currentType = currentType.BaseType;
            }
        }

        public static bool IsChangeable(this ISymbol symbol) =>
            !symbol.IsAbstract
            && !symbol.IsVirtual
            && symbol.GetInterfaceMember() == null
            && symbol.GetOverriddenMember() == null;

        public static bool IsExtensionOn(this IMethodSymbol methodSymbol, KnownType type)
        {
            if (!(methodSymbol is {IsExtensionMethod: true}))
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

        public static IEnumerable<IParameterSymbol> GetParameters(this ISymbol symbol) =>
            symbol.Kind switch
            {
                SymbolKind.Method => ((IMethodSymbol)symbol).Parameters,
                SymbolKind.Property => ((IPropertySymbol)symbol).Parameters,
                _ => Enumerable.Empty<IParameterSymbol>()
            };

        public static bool IsAnyAttributeInOverridingChain(IPropertySymbol propertySymbol) =>
            IsAnyAttributeInOverridingChain(propertySymbol, property => property.OverriddenProperty);

        public static bool IsAnyAttributeInOverridingChain(IMethodSymbol methodSymbol) =>
            IsAnyAttributeInOverridingChain(methodSymbol, method => method.OverriddenMethod);

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

            return effectiveAccessibility == Accessibility.Public
                   || effectiveAccessibility == Accessibility.Protected
                   || effectiveAccessibility == Accessibility.ProtectedOrInternal;
        }

        public static bool IsConstructor(this ISymbol symbol) =>
            symbol.Kind == SymbolKind.Method && symbol.Name == ".ctor";

        public static bool IsDestructor(this IMethodSymbol method) =>
            method.MethodKind == MethodKind.Destructor;

        public static bool IsSameNamespace(this INamespaceSymbol namespace1, INamespaceSymbol namespace2) =>
            (namespace1.IsGlobalNamespace && namespace2.IsGlobalNamespace)
            || (namespace1.Name.Equals(namespace2.Name)
                && namespace1.ContainingNamespace != null
                && namespace2.ContainingNamespace != null
                && IsSameNamespace(namespace1.ContainingNamespace, namespace2.ContainingNamespace));

        public static bool IsSameOrAncestorOf(this INamespaceSymbol thisNamespace, INamespaceSymbol namespaceToCheck) =>
            IsSameNamespace(thisNamespace, namespaceToCheck)
            || (namespaceToCheck.ContainingNamespace != null && IsSameOrAncestorOf(thisNamespace, namespaceToCheck.ContainingNamespace));

        internal static IEnumerable<AttributeData> GetAttributes(this ISymbol symbol, KnownType attributeType) =>
            symbol?.GetAttributes().Where(a => a.AttributeClass.Is(attributeType))
            ?? Enumerable.Empty<AttributeData>();

        internal static IEnumerable<AttributeData> GetAttributes(this ISymbol symbol, ImmutableArray<KnownType> attributeTypes) =>
            symbol?.GetAttributes().Where(a => a.AttributeClass.IsAny(attributeTypes))
            ?? Enumerable.Empty<AttributeData>();

        internal static bool AnyAttributeDerivesFrom(this ISymbol symbol, KnownType attributeType) =>
            symbol?.GetAttributes().Any(a => a.AttributeClass.DerivesFrom(attributeType)) ?? false;

        internal static bool AnyAttributeDerivesFromAny(this ISymbol symbol, ImmutableArray<KnownType> attributeTypes) =>
            symbol?.GetAttributes().Any(a => a.AttributeClass.DerivesFromAny(attributeTypes)) ?? false;

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

        private static bool CanBeInterfaceMember(ISymbol symbol) =>
            symbol.Kind == SymbolKind.Method
            || symbol.Kind == SymbolKind.Property
            || symbol.Kind == SymbolKind.Event;
    }
}
