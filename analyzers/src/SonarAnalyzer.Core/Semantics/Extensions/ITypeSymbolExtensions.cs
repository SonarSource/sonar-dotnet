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

namespace SonarAnalyzer.Core.Semantics.Extensions;

public static class ITypeSymbolExtensions
{
    extension(ITypeSymbol symbol)
    {
        public bool IsInterface => symbol is { TypeKind: TypeKind.Interface };

        public bool IsClass => symbol is { TypeKind: TypeKind.Class };

        public bool IsStruct => symbol is { TypeKind: TypeKind.Struct } or ITypeParameterSymbol { IsValueType: true };

        public bool IsClassOrStruct => symbol is { IsStruct: true } or { IsClass: true };

        public bool IsExtensionBlock => symbol is { TypeKind: TypeKindEx.Extension };

        public bool IsNullableValueType => symbol is { IsStruct: true } and ({ SpecialType: SpecialType.System_Nullable_T } or { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T });

        public bool IsNonNullableValueType => symbol is { IsStruct: true, IsNullableValueType: false };

        public bool IsEnum =>
            symbol switch
            {
                { TypeKind: TypeKind.Enum } => true,
                ITypeParameterSymbol { HasReferenceTypeConstraint: false, ConstraintTypes: { IsEmpty: false } constraintTypes } => constraintTypes.Any(x => x.SpecialType == SpecialType.System_Enum),
                _ => false,
            };

        public bool CanBeNull => symbol is { IsReferenceType: true } or { IsNullableValueType: true };

        public bool IsNullableBoolean => symbol.IsNullableOf(KnownType.System_Boolean);

        public IEnumerable<INamedTypeSymbol> SelfAndBaseTypes
        {
            get
            {
                if (symbol is null)
                {
                    yield break;
                }

                var currentType = symbol;
                while (currentType?.Kind == SymbolKind.NamedType)
                {
                    yield return (INamedTypeSymbol)currentType;
                    currentType = currentType.BaseType;
                }
            }
        }

        public bool Is(TypeKind typeKind) =>
            symbol?.TypeKind == typeKind;

        public bool Is(KnownType type) =>
            symbol is not null && type.Matches(symbol);

        public bool IsAny(params KnownType[] types)
        {
            if (symbol is null)
            {
                return false;
            }

            // For is twice as fast as foreach on ImmutableArray so don't use Linq here
            for (var i = 0; i < types.Length; i++)
            {
                if (types[i].Matches(symbol))
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsAny(ImmutableArray<KnownType> types)
        {
            if (symbol is null)
            {
                return false;
            }

            // For is twice as fast as foreach on ImmutableArray so don't use Linq here
            for (var i = 0; i < types.Length; i++)
            {
                if (types[i].Matches(symbol))
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsNullableOfAny(ImmutableArray<KnownType> argumentTypes) =>
            NullableTypeArgument(symbol).IsAny(argumentTypes);

        public bool IsNullableOf(KnownType typeArgument) =>
            NullableTypeArgument(symbol).Is(typeArgument);

        public bool Implements(KnownType type) =>
            symbol is not null
            && symbol.AllInterfaces.Any(x => x.ConstructedFrom.Is(type));

        public bool ImplementsAny(ImmutableArray<KnownType> types) =>
            symbol is not null
            && symbol.AllInterfaces.Any(x => x.ConstructedFrom.IsAny(types));

        public bool ImplementsAny(params KnownType[] types) =>
            symbol is not null
            && symbol.AllInterfaces.Any(x => x.ConstructedFrom.IsAny(types));

        public bool DerivesFrom(KnownType type)
        {
            var currentType = symbol;
            while (currentType is not null)
            {
                if (currentType.Is(type))
                {
                    return true;
                }
                currentType = currentType.BaseType?.ConstructedFrom;
            }

            return false;
        }

        public bool DerivesFrom(ITypeSymbol type)
        {
            var currentType = symbol;
            while (currentType is not null)
            {
                if (currentType.Equals(type) || (currentType is INamedTypeSymbol { ConstructedFrom: { } constructedFrom } && constructedFrom.Equals(type)))
                {
                    return true;
                }
                currentType = currentType.BaseType?.ConstructedFrom;
            }

            return false;
        }

        public bool DerivesFromAny(ImmutableArray<KnownType> baseTypes)
        {
            var currentType = symbol;
            while (currentType is not null)
            {
                if (currentType.IsAny(baseTypes))
                {
                    return true;
                }
                currentType = currentType.BaseType?.ConstructedFrom;
            }

            return false;
        }

        /// <summary>
        /// Returns the underlying value type <c>T</c> of <see cref="Nullable{T}"/>, or <see langword="null"/> if
        /// <paramref name="symbol"/> is not a nullable value type. Does not affect nullable reference types (NRT annotations).
        /// </summary>
        public ITypeSymbol NullableUnderlyingType() =>
            symbol is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T, TypeArguments.Length: 1 } nullable
                ? nullable.TypeArguments[0]
                : null;

        /// <summary>
        /// Returns the underlying value type <c>T</c> of <see cref="Nullable{T}"/>, or <paramref name="symbol"/> unchanged
        /// if it is not a nullable value type. Does not affect nullable reference types (NRT annotations).
        /// </summary>
        public ITypeSymbol NullableUnderlyingTypeOrSelf() =>
            symbol.NullableUnderlyingType() ?? symbol;

        public bool DerivesOrImplements(KnownType baseType) =>
            symbol.Implements(baseType) || symbol.DerivesFrom(baseType);

        public bool DerivesOrImplements(ITypeSymbol baseType) =>
            symbol.Implements(baseType) || symbol.DerivesFrom(baseType);

        public bool DerivesOrImplementsAny(ImmutableArray<KnownType> baseTypes) =>
            symbol.ImplementsAny(baseTypes) || symbol.DerivesFromAny(baseTypes);

        private bool Implements(ISymbol type) =>
            symbol is not null
            && symbol.AllInterfaces.Any(x => type.IsDefinition ? x.OriginalDefinition.Equals(type) : x.Equals(type));
    }

    extension(ISymbol symbol)
    {
        public ITypeSymbol SymbolType =>
            symbol switch
            {
                ILocalSymbol x => x.Type,
                IFieldSymbol x => x.Type,
                IPropertySymbol x => x.Type,
                IParameterSymbol x => x.Type,
                IAliasSymbol x => x.Target as ITypeSymbol,
                IMethodSymbol { MethodKind: MethodKind.Constructor } x => x.ContainingType,
                IMethodSymbol x => x.ReturnType,
                ITypeSymbol x => x,
                _ => null,
            };
    }

    private static ITypeSymbol NullableTypeArgument(ITypeSymbol type) =>
        type is INamedTypeSymbol namedType && namedType.OriginalDefinition.Is(KnownType.System_Nullable_T)
            ? namedType.TypeArguments[0]
            : null;
}
