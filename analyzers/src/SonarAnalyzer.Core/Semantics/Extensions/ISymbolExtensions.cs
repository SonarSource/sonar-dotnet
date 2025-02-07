/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using System.Diagnostics.CodeAnalysis;

namespace SonarAnalyzer.Core.Semantics.Extensions;

public static class ISymbolExtensions
{
    public static bool HasAnyAttribute(this ISymbol symbol, ImmutableArray<KnownType> types) =>
        symbol.GetAttributes(types).Any();

    public static bool HasAttribute(this ISymbol symbol, KnownType type) =>
        symbol.GetAttributes(type).Any();

    public static SyntaxNode GetFirstSyntaxRef(this ISymbol symbol) =>
        symbol?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax();

    public static bool IsAutoProperty(this ISymbol symbol) =>
        symbol.Kind == SymbolKind.Property && symbol.ContainingType.GetMembers().OfType<IFieldSymbol>().Any(x => symbol.Equals(x.AssociatedSymbol));

    public static bool IsTopLevelMain(this ISymbol symbol) =>
        symbol is IMethodSymbol { Name: TopLevelStatements.MainMethodImplicitName };

    public static bool IsGlobalNamespace(this ISymbol symbol) =>
        symbol is INamespaceSymbol { Name: "" };

    public static bool IsInSameAssembly(this ISymbol symbol, ISymbol anotherSymbol) =>
        symbol.ContainingAssembly.Equals(anotherSymbol.ContainingAssembly);

    public static bool HasNotNullAttribute(this ISymbol parameter) =>
        parameter.GetAttributes() is { Length: > 0 } attributes && attributes.Any(IsNotNullAttribute);

    // https://github.com/dotnet/roslyn/blob/2a594fa2157a734a988f7b5dbac99484781599bd/src/Workspaces/SharedUtilitiesAndExtensions/Compiler/Core/Extensions/ISymbolExtensions.cs#L93
    [ExcludeFromCodeCoverage]
    public static ImmutableArray<ISymbol> ExplicitOrImplicitInterfaceImplementations(this ISymbol symbol)
    {
        if (symbol.Kind is not SymbolKind.Method and not SymbolKind.Property and not SymbolKind.Event)
        {
            return ImmutableArray<ISymbol>.Empty;
        }

        var containingType = symbol.ContainingType;
        var query = from iface in containingType.AllInterfaces
                    from interfaceMember in iface.GetMembers()
                    let impl = containingType.FindImplementationForInterfaceMember(interfaceMember)
                    where symbol.Equals(impl)
                    select interfaceMember;
        return query.ToImmutableArray();
    }

    public static bool HasContainingType(this ISymbol method, KnownType containingType, bool checkDerivedTypes) =>
        checkDerivedTypes
            ? method.ContainingType.DerivesOrImplements(containingType)
            : method.ContainingType.Is(containingType);

    public static bool IsInType(this ISymbol symbol, KnownType type) =>
        symbol is not null && symbol.ContainingType.Is(type);

    public static bool IsInType(this ISymbol symbol, ITypeSymbol type) =>
        symbol?.ContainingType is not null && symbol.ContainingType.Equals(type);

    public static bool IsInType(this ISymbol symbol, ImmutableArray<KnownType> types) =>
        symbol is not null && symbol.ContainingType.IsAny(types);

    public static T GetInterfaceMember<T>(this T symbol) where T : class, ISymbol =>
        symbol is null || symbol.IsOverride || !CanBeInterfaceMember(symbol)
            ? null
            : symbol.ContainingType
                .AllInterfaces
                .SelectMany(x => x.GetMembers())
                .OfType<T>()
                .FirstOrDefault(x => symbol.Equals(symbol.ContainingType.FindImplementationForInterfaceMember(x)));

    public static T GetOverriddenMember<T>(this T symbol) where T : class, ISymbol =>
        symbol is { IsOverride: true }
            ? symbol.Kind switch
            {
                SymbolKind.Method => (T)((IMethodSymbol)symbol).OverriddenMethod,
                SymbolKind.Property => (T)((IPropertySymbol)symbol).OverriddenProperty,
                SymbolKind.Event => (T)((IEventSymbol)symbol).OverriddenEvent,
                _ => throw new ArgumentException($"Only methods, properties and events can be overridden. {typeof(T).Name} was provided", nameof(symbol))
            }
            : null;

    public static bool IsChangeable(this ISymbol symbol) =>
        !symbol.IsAbstract
        && !symbol.IsVirtual
        && symbol.GetInterfaceMember() is null
        && symbol.GetOverriddenMember() is null;

    public static IEnumerable<IParameterSymbol> GetParameters(this ISymbol symbol) =>
        symbol.Kind switch
        {
            SymbolKind.Method => ((IMethodSymbol)symbol).Parameters,
            SymbolKind.Property => ((IPropertySymbol)symbol).Parameters,
            _ => Enumerable.Empty<IParameterSymbol>()
        };

    public static Accessibility GetEffectiveAccessibility(this ISymbol symbol)
    {
        if (symbol is null)
        {
            return Accessibility.NotApplicable;
        }

        var result = symbol.DeclaredAccessibility;
        if (result == Accessibility.Private)
        {
            return Accessibility.Private;
        }

        for (var container = symbol.ContainingType; container is not null; container = container.ContainingType)
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
        symbol.GetEffectiveAccessibility() is Accessibility.Public or Accessibility.Protected or Accessibility.ProtectedOrInternal;

    public static bool IsConstructor(this ISymbol symbol) =>
        symbol.Kind == SymbolKind.Method && symbol.Name == ".ctor";

    public static IEnumerable<AttributeData> GetAttributes(this ISymbol symbol, KnownType attributeType) =>
        symbol?.GetAttributes().Where(x => x.AttributeClass.Is(attributeType)) ?? [];

    public static IEnumerable<AttributeData> GetAttributes(this ISymbol symbol, ImmutableArray<KnownType> attributeTypes) =>
        symbol?.GetAttributes().Where(x => x.AttributeClass.IsAny(attributeTypes)) ?? [];

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

    public static bool AnyAttributeDerivesFrom(this ISymbol symbol, KnownType attributeType) =>
        symbol?.GetAttributes().Any(x => x.AttributeClass.DerivesFrom(attributeType)) ?? false;

    public static bool AnyAttributeDerivesFromAny(this ISymbol symbol, ImmutableArray<KnownType> attributeTypes) =>
        symbol?.GetAttributes().Any(x => x.AttributeClass.DerivesFromAny(attributeTypes)) ?? false;

    public static bool AnyAttributeDerivesFromOrImplementsAny(this ISymbol symbol, ImmutableArray<KnownType> attributeTypesOrInterfaces) =>
        symbol?.GetAttributes().Any(x => x.AttributeClass.DerivesOrImplementsAny(attributeTypesOrInterfaces)) ?? false;

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

    public static bool IsSerializableMember(this ISymbol symbol) =>
        symbol is IFieldSymbol or IPropertySymbol { SetMethod: not null }
        && symbol.ContainingType.GetAttributes().Any(x => x.AttributeClass.Is(KnownType.System_SerializableAttribute))
        && !symbol.GetAttributes().Any(x => x.AttributeClass.Is(KnownType.System_NonSerializedAttribute));

    public static bool IsAnyAttributeInOverridingChain<TSymbol>(this TSymbol symbol, Func<TSymbol, TSymbol> overriddenMember)
        where TSymbol : class, ISymbol
    {
        var currentSymbol = symbol;
        while (currentSymbol is not null)
        {
            if (currentSymbol.GetAttributes().Any())
            {
                return true;
            }
            if (!currentSymbol.IsOverride)
            {
                return false;
            }
            currentSymbol = overriddenMember(currentSymbol);
        }
        return false;
    }

    /// <summary>
    /// Retrieves all parts of a symbol. For partial methods or properties, both the definition and implementation parts are included. For other symbols, the symbol itself is returned.
    /// </summary>
    public static IEnumerable<ISymbol> AllPartialParts(this ISymbol symbol)
    {
        switch (symbol)
        {
            case IMethodSymbol method:
                yield return method;
                if (method.PartialImplementationPart is { } implementation)
                {
                    yield return implementation;
                }
                else if (method.PartialDefinitionPart is { } definition)
                {
                    yield return definition;
                }
                break;
            case IPropertySymbol property:
                yield return property;
                if (property.PartialImplementationPart() is { } propertyImplementation)
                {
                    yield return propertyImplementation;
                }
                else if (property.PartialDefinitionPart() is { } propertyDefinition)
                {
                    yield return propertyDefinition;
                }
                break;
            default:
                yield return symbol;
                break;
        }
    }

    private static bool CanBeInterfaceMember(ISymbol symbol) =>
        symbol.Kind == SymbolKind.Method
        || symbol.Kind == SymbolKind.Property
        || symbol.Kind == SymbolKind.Event;

    // https://docs.microsoft.com/dotnet/api/microsoft.validatednotnullattribute
    // https://docs.microsoft.com/dotnet/csharp/language-reference/attributes/nullable-analysis#postconditions-maybenull-and-notnull
    // https://www.jetbrains.com/help/resharper/Reference__Code_Annotation_Attributes.html#NotNullAttribute
    private static bool IsNotNullAttribute(AttributeData attribute) =>
        attribute.HasAnyName("ValidatedNotNullAttribute", "NotNullAttribute");
}
