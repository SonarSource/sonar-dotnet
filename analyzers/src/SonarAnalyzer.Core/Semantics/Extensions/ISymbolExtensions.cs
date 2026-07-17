/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
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
    extension(ISymbol symbol)
    {
        public SyntaxNode FirstSyntaxRef => symbol?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax();

        public bool IsAutoProperty => symbol.Kind == SymbolKind.Property && symbol.ContainingType.GetMembers().OfType<IFieldSymbol>().Any(x => symbol.Equals(x.AssociatedSymbol));

        public bool IsTopLevelMain => symbol is IMethodSymbol { Name: TopLevelStatements.MainMethodImplicitName };

        public bool IsGlobalNamespace => symbol is INamespaceSymbol { Name: "" };

        public bool HasNotNullAttribute => symbol.GetAttributes() is { Length: > 0 } attributes && attributes.Any(IsNotNullAttribute);

        public IEnumerable<IParameterSymbol> Parameters =>
            symbol.Kind switch
            {
                SymbolKind.Method => ((IMethodSymbol)symbol).Parameters,
                SymbolKind.Property => ((IPropertySymbol)symbol).Parameters,
                _ => Enumerable.Empty<IParameterSymbol>()
            };

        public Accessibility EffectiveAccessibility
        {
            get
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
        }

        public bool IsPubliclyAccessible => symbol.EffectiveAccessibility is Accessibility.Public or Accessibility.Protected or Accessibility.ProtectedOrInternal;

        public bool IsConstructor => symbol.Kind == SymbolKind.Method && symbol.Name == ".ctor";

        /// <summary>
        /// Returns attributes for the symbol by also respecting <see cref="AttributeUsageAttribute.Inherited"/>.
        /// The returned <see cref="AttributeData"/> is consistent with the results from <see cref="MemberInfo.GetCustomAttributes(bool)"/>.
        /// </summary>
        public IEnumerable<AttributeData> AttributesWithInherited
        {
            get
            {
                foreach (var attribute in symbol.GetAttributes())
                {
                    yield return attribute;
                }

                var baseSymbol = BaseSymbol(symbol);
                while (baseSymbol is not null)
                {
                    foreach (var attribute in baseSymbol.GetAttributes().Where(x => x.HasAttributeUsageInherited))
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
        }

        public string Classification =>
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

        public bool IsSerializableMember =>
            symbol is IFieldSymbol or IPropertySymbol { SetMethod: not null }
            && symbol.ContainingType.GetAttributes().Any(x => x.AttributeClass.Is(KnownType.System_SerializableAttribute))
            && !symbol.GetAttributes().Any(x => x.AttributeClass.Is(KnownType.System_NonSerializedAttribute));

        /// <summary>
        /// Retrieves all parts of a symbol. For partial methods or properties, both the definition and implementation parts are included. For other symbols, the symbol itself is returned.
        /// </summary>
        public IEnumerable<ISymbol> AllPartialParts
        {
            get
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
        }

        public bool HasAnyAttribute(ImmutableArray<KnownType> types) =>
            symbol.GetAttributes(types).Any();

        public bool HasAttribute(KnownType type) =>
            symbol.GetAttributes(type).Any();

        public bool IsInSameAssembly(ISymbol anotherSymbol) =>
            symbol.ContainingAssembly.Equals(anotherSymbol.ContainingAssembly);

        // "Countable" per the C# 8 ranges/indices proposal: a property named Count or Length with an accessible int getter.
        // https://github.com/dotnet/csharplang/blob/e145230405eabef04a460003a20825fecce7f4d5/proposals/csharp-8.0/ranges.md#implicit-index-support
        public bool IsCountable()
        {
            return symbol is ITypeSymbol type && (HasCountableMember(type, "Count") || HasCountableMember(type, "Length"));

            // GetMembers(name) is a dictionary lookup on the type's cached members-by-name map, unlike GetMembers()
            // which forces scanning every member of the type just to find the one or two we care about.
            static bool HasCountableMember(ITypeSymbol type, string name) =>
                type.GetMembers(name).OfType<IPropertySymbol>()
                    .Any(x => x is { Type: INamedTypeSymbol { SpecialType: SpecialType.System_Int32 }, IsStatic: false, GetMethod: not null });
        }

        // https://github.com/dotnet/roslyn/blob/2a594fa2157a734a988f7b5dbac99484781599bd/src/Workspaces/SharedUtilitiesAndExtensions/Compiler/Core/Extensions/ISymbolExtensions.cs#L93
        [ExcludeFromCodeCoverage]
        public ImmutableArray<ISymbol> ExplicitOrImplicitInterfaceImplementations()
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

        public bool HasContainingType(KnownType containingType, bool checkDerivedTypes) =>
            checkDerivedTypes
                ? symbol.ContainingType.DerivesOrImplements(containingType)
                : symbol.ContainingType.Is(containingType);

        public bool IsInType(KnownType type) =>
            symbol is not null && symbol.ContainingType.Is(type);

        public bool IsInType(ITypeSymbol type) =>
            symbol?.ContainingType is not null && symbol.ContainingType.Equals(type);

        public bool IsInType(ImmutableArray<KnownType> types) =>
            symbol is not null && symbol.ContainingType.IsAny(types);

        public bool IsChangeable() =>
            symbol is { IsAbstract: false, IsVirtual: false, OverriddenMember: null }
            && symbol.InterfaceMembers().IsEmpty;

        public IEnumerable<AttributeData> GetAttributes(KnownType attributeType) =>
            symbol?.GetAttributes().Where(x => x.AttributeClass.Is(attributeType)) ?? [];

        public IEnumerable<AttributeData> GetAttributes(ImmutableArray<KnownType> attributeTypes) =>
            symbol?.GetAttributes().Where(x => x.AttributeClass.IsAny(attributeTypes)) ?? [];

        public bool AnyAttributeDerivesFrom(KnownType attributeType) =>
            symbol?.GetAttributes().Any(x => x.AttributeClass.DerivesFrom(attributeType)) ?? false;

        public bool AnyAttributeDerivesFromAny(ImmutableArray<KnownType> attributeTypes) =>
            symbol?.GetAttributes().Any(x => x.AttributeClass.DerivesFromAny(attributeTypes)) ?? false;

        public bool AnyAttributeDerivesFromOrImplementsAny(ImmutableArray<KnownType> attributeTypesOrInterfaces) =>
            symbol?.GetAttributes().Any(x => x.AttributeClass.DerivesOrImplementsAny(attributeTypesOrInterfaces)) ?? false;
    }

    extension<T>(T symbol) where T : class, ISymbol
    {
        public T OverriddenMember =>
            symbol is { IsOverride: true }
                ? symbol.Kind switch
                {
                    SymbolKind.Method => (T)((IMethodSymbol)symbol).OverriddenMethod,
                    SymbolKind.Property => (T)((IPropertySymbol)symbol).OverriddenProperty,
                    SymbolKind.Event => (T)((IEventSymbol)symbol).OverriddenEvent,
                    _ => throw new ArgumentException($"Only methods, properties and events can be overridden. {typeof(T).Name} was provided", nameof(symbol))
                }
                : null;

        public IEnumerable<T> OverriddenMembersAndSelf
        {
            get
            {
                yield return symbol;
                var overriden = symbol.OverriddenMember;
                while (overriden is not null)
                {
                    yield return overriden;
                    overriden = overriden.OverriddenMember;
                }
            }
        }

        public bool IsAnyAttributeInOverridingChain
        {
            get
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
                    currentSymbol = currentSymbol.OverriddenMember;
                }
                return false;
            }
        }

        /// <summary>
        /// Returns all interface members this member implements.
        /// A single member can implement members from multiple interface.
        /// </summary>
        public IEnumerable<T> InterfaceMembers() =>
            symbol switch
            {
                null => [],
                { } when !CanBeInterfaceMember(symbol) => [],
                _ => symbol.ContainingType
                    .AllInterfaces
                    .SelectMany(x => x.GetMembers())
                    .OfType<T>()
                    .Where(x =>
                        symbol.OverriddenMembersAndSelf.Any(m =>
                            m.Equals(m.ContainingType.FindImplementationForInterfaceMember(x)))),
            };
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
