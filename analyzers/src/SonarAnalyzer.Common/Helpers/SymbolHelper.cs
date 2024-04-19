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

using System.Reflection;

namespace SonarAnalyzer.Helpers
{
    internal static class SymbolHelper
    {
        private static readonly PropertyInfo ITypeSymbolIsRecord = typeof(ITypeSymbol).GetProperty("IsRecord");

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
            if (!(symbol is { IsOverride: true }))
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

        public static bool IsPubliclyAccessible(this ISymbol symbol) =>
            GetEffectiveAccessibility(symbol) is Accessibility.Public or Accessibility.Protected or Accessibility.ProtectedOrInternal;

        public static bool IsConstructor(this ISymbol symbol) =>
            symbol.Kind == SymbolKind.Method && symbol.Name == ".ctor";

        public static bool IsStaticConstructor(this ISymbol symbol) =>
            symbol.Kind == SymbolKind.Method && ((IMethodSymbol)symbol).MethodKind == MethodKind.StaticConstructor;

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

        /// <summary>
        /// Returns attributes for the symbol by also respecting <see cref="AttributeUsageAttribute.Inherited"/>.
        /// The returned <see cref="AttributeData"/> is consistent with the results from <see cref="MemberInfo.GetCustomAttributes(bool)"/>.
        /// </summary>
        public static IEnumerable<AttributeData> GetAttributesWithInherited(this ISymbol symbol)
        {
            foreach (var attribute in symbol.GetAttributes())
            {
                yield return attribute;
            }

            var baseSymbol = BaseSymbol(symbol);
            while (baseSymbol is not null)
            {
                foreach (var attribute in baseSymbol.GetAttributes().Where(x => x.HasAttributeUsageInherited()))
                {
                    yield return attribute;
                }

                baseSymbol = BaseSymbol(baseSymbol);
            }

            static ISymbol BaseSymbol(ISymbol symbol) =>
                symbol switch
                {
                    INamedTypeSymbol namedType => namedType.BaseType,
                    IMethodSymbol { OriginalDefinition: { } originalDefinition } method when !method.Equals(originalDefinition) => BaseSymbol(originalDefinition),
                    IMethodSymbol { OverriddenMethod: { } overridenMethod } => overridenMethod,
                    // Support for other kinds of symbols needs to be implemented/tested as needed. A full list can be found here:
                    // https://learn.microsoft.com/dotnet/api/system.attributetargets
                    _ => null,
                };
        }

        internal static bool AnyAttributeDerivesFrom(this ISymbol symbol, KnownType attributeType) =>
            symbol?.GetAttributes().Any(a => a.AttributeClass.DerivesFrom(attributeType)) ?? false;

        internal static bool AnyAttributeDerivesFromAny(this ISymbol symbol, ImmutableArray<KnownType> attributeTypes) =>
            symbol?.GetAttributes().Any(a => a.AttributeClass.DerivesFromAny(attributeTypes)) ?? false;

        internal static bool AnyAttributeDerivesFromOrImplementsAny(this ISymbol symbol, ImmutableArray<KnownType> attributeTypesOrInterfaces) =>
            symbol?.GetAttributes().Any(a => a.AttributeClass.DerivesOrImplementsAny(attributeTypesOrInterfaces)) ?? false;

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

        public static string GetClassification(this ISymbol symbol) =>
            symbol switch
            {
                { Kind: SymbolKind.Alias } => "alias",
                { Kind: SymbolKind.ArrayType } => "array",
                { Kind: SymbolKind.Assembly } => "assembly",
                { Kind: SymbolKindEx.Discard } => "discard",
                { Kind: SymbolKind.DynamicType } => "dynamic",
                { Kind: SymbolKind.ErrorType } => "error",
                { Kind: SymbolKind.Event } => "event",
                { Kind: SymbolKindEx.FunctionPointerType } => "function pointer",
                { Kind: SymbolKind.Field } => "field",
                { Kind: SymbolKind.Label } => "label",
                { Kind: SymbolKind.Local } => "local",
                { Kind: SymbolKind.Namespace } => "namespace",
                { Kind: SymbolKind.NetModule } => "netmodule",
                { Kind: SymbolKind.PointerType } => "pointer",
                { Kind: SymbolKind.Preprocessing } => "preprocessing",
                { Kind: SymbolKind.Parameter } => "parameter",
                { Kind: SymbolKind.RangeVariable } => "range variable",
                { Kind: SymbolKind.Property } => "property",
                { Kind: SymbolKind.TypeParameter } => "type parameter",
                IMethodSymbol methodSymbol => methodSymbol switch
                {
                    { MethodKind: MethodKind.BuiltinOperator or MethodKind.UserDefinedOperator or MethodKind.Conversion } => "operator",
                    { MethodKind: MethodKind.Constructor or MethodKind.StaticConstructor or MethodKind.SharedConstructor } => "constructor",
                    { MethodKind: MethodKind.Destructor } => "destructor",
                    { MethodKind: MethodKind.PropertyGet } => "getter",
                    { MethodKind: MethodKind.PropertySet } => "setter",
                    { MethodKind: MethodKindEx.LocalFunction } => "local function",
                    _ => "method",
                },
                INamedTypeSymbol namedTypeSymbol => namedTypeSymbol switch
                {
                    { TypeKind: TypeKind.Array } => "array",
                    { TypeKind: TypeKind.Class } namedType => namedType.IsRecord() ? "record" : "class",
                    { TypeKind: TypeKind.Dynamic } => "dynamic",
                    { TypeKind: TypeKind.Delegate } => "delegate",
                    { TypeKind: TypeKind.Enum } => "enum",
                    { TypeKind: TypeKind.Error } => "error",
                    { TypeKind: TypeKindEx.FunctionPointer } => "function pointer",
                    { TypeKind: TypeKind.Interface } => "interface",
                    { TypeKind: TypeKind.Module } => "module",
                    { TypeKind: TypeKind.Pointer } => "pointer",
                    { TypeKind: TypeKind.Struct or TypeKind.Structure } namedType => namedType.IsRecord() ? "record struct" : "struct",
                    { TypeKind: TypeKind.Submission } => "submission",
                    { TypeKind: TypeKind.TypeParameter } => "type parameter",
                    { TypeKind: TypeKind.Unknown } => "unknown",
#if DEBUG
                    _ => throw new NotSupportedException($"symbol is of a not yet supported kind."),
#else
                    _ => "type",
#endif
                },
#if DEBUG
                _ => throw new NotSupportedException($"symbol is of a not yet supported kind."),
#else
                _ => "symbol",
#endif

            };

        public static bool IsRecord(this ITypeSymbol typeSymbol)
            => ITypeSymbolIsRecord?.GetValue(typeSymbol) is true;

        public static bool IsSerializableMember(this ISymbol symbol) =>
            symbol is IFieldSymbol or IPropertySymbol { SetMethod: not null }
            && symbol.ContainingType.GetAttributes().Any(x => x.AttributeClass.Is(KnownType.System_SerializableAttribute))
            && !symbol.GetAttributes().Any(x => x.AttributeClass.Is(KnownType.System_NonSerializedAttribute));
    }
}
