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

using Microsoft.CodeAnalysis;
using static Google.Protobuf.WellKnownTypes.Field;

namespace SonarAnalyzer.Helpers;

public static class TypeHelper
{
    #region TypeKind

    public static bool IsInterface(this ITypeSymbol self) =>
        self is { TypeKind: TypeKind.Interface };

    public static bool IsClass(this ITypeSymbol self) =>
        self is { TypeKind: TypeKind.Class };

    public static bool IsStruct(this ITypeSymbol self) =>
        self switch
        {
            { TypeKind: TypeKind.Struct } => true,
            ITypeParameterSymbol { IsValueType: true } => true,
            _ => false,
        };

    public static bool IsClassOrStruct(this ITypeSymbol self) =>
        self.IsStruct() || self.IsClass();

    public static bool Is(this ITypeSymbol self, TypeKind typeKind) =>
        self?.TypeKind == typeKind;

    public static bool IsNullableValueType(this ITypeSymbol self) =>
        self.IsStruct() && self is { SpecialType: SpecialType.System_Nullable_T } or { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T };

    public static bool IsNonNullableValueType(this ITypeSymbol self) =>
        self.IsStruct() && !self.IsNullableValueType();

    public static bool IsEnum(this ITypeSymbol self) =>
        self switch
        {
            { TypeKind: TypeKind.Enum } => true,
            ITypeParameterSymbol { HasReferenceTypeConstraint: false, ConstraintTypes: { IsEmpty: false } constraintTypes } => constraintTypes.Any(x => x.SpecialType == SpecialType.System_Enum),
            _ => false,
        };

    public static bool CanBeNull(this ITypeSymbol self) =>
        self is { IsReferenceType: true } || self.IsNullableValueType();

    #endregion TypeKind

    #region TypeName

    public static bool Is(this ITypeSymbol typeSymbol, KnownType type) =>
        typeSymbol != null && type.Matches(typeSymbol);

    public static bool IsAny(this ITypeSymbol typeSymbol, params KnownType[] types)
    {
        if (typeSymbol == null)
        {
            return false;
        }

        // For is twice as fast as foreach on ImmutableArray so don't use Linq here
        for (var i = 0; i < types.Length; i++)
        {
            if (types[i].Matches(typeSymbol))
            {
                return true;
            }
        }

        return false;
    }

    public static bool IsAny(this ITypeSymbol typeSymbol, ImmutableArray<KnownType> types)
    {
        if (typeSymbol == null)
        {
            return false;
        }

        // For is twice as fast as foreach on ImmutableArray so don't use Linq here
        for (var i = 0; i < types.Length; i++)
        {
            if (types[i].Matches(typeSymbol))
            {
                return true;
            }
        }

        return false;
    }

    public static bool IsNullableOfAny(this ITypeSymbol type, ImmutableArray<KnownType> argumentTypes) =>
        NullableTypeArgument(type).IsAny(argumentTypes);

    public static bool IsNullableOf(this ITypeSymbol type, KnownType typeArgument) =>
        NullableTypeArgument(type).Is(typeArgument);

    public static bool IsType(this IParameterSymbol parameter, KnownType type) =>
        parameter != null && parameter.Type.Is(type);

    public static bool IsInType(this ISymbol symbol, KnownType type) =>
        symbol != null && symbol.ContainingType.Is(type);

    public static bool IsInType(this ISymbol symbol, ITypeSymbol type) =>
        symbol?.ContainingType != null && symbol.ContainingType.Equals(type);

    public static bool IsInType(this ISymbol symbol, ImmutableArray<KnownType> types) =>
        symbol != null && symbol.ContainingType.IsAny(types);

    #endregion TypeName

    public static bool IsNullableBoolean(this ITypeSymbol type) =>
        type.IsNullableOf(KnownType.System_Boolean);

    public static bool Implements(this ITypeSymbol typeSymbol, KnownType type) =>
        typeSymbol is { }
        && typeSymbol.AllInterfaces.Any(x => x.ConstructedFrom.Is(type));

    private static bool Implements(this ITypeSymbol typeSymbol, ISymbol type) =>
        typeSymbol is { }
        && typeSymbol.AllInterfaces.Any(x => type.IsDefinition ? x.OriginalDefinition.Equals(type) : x.Equals(type));

    public static bool ImplementsAny(this ITypeSymbol typeSymbol, ImmutableArray<KnownType> types) =>
        typeSymbol is { }
        && typeSymbol.AllInterfaces.Any(symbol => symbol.ConstructedFrom.IsAny(types));

    public static bool DerivesFrom(this ITypeSymbol typeSymbol, KnownType type)
    {
        var currentType = typeSymbol;
        while (currentType != null)
        {
            if (currentType.Is(type))
            {
                return true;
            }
            currentType = currentType.BaseType?.ConstructedFrom;
        }

        return false;
    }

    public static bool DerivesFrom(this ITypeSymbol typeSymbol, ITypeSymbol type)
    {
        var currentType = typeSymbol;
        while (currentType != null)
        {
            if (currentType.Equals(type))
            {
                return true;
            }
            currentType = currentType.BaseType?.ConstructedFrom;
        }

        return false;
    }

    public static bool DerivesFromAny(this ITypeSymbol typeSymbol, ImmutableArray<KnownType> baseTypes)
    {
        var currentType = typeSymbol;
        while (currentType != null)
        {
            if (currentType.IsAny(baseTypes))
            {
                return true;
            }
            currentType = currentType.BaseType?.ConstructedFrom;
        }

        return false;
    }

    public static bool DerivesOrImplements(this ITypeSymbol type, KnownType baseType) =>
        type.Implements(baseType) || type.DerivesFrom(baseType);

    public static bool DerivesOrImplements(this ITypeSymbol type, ITypeSymbol baseType) =>
        type.Implements(baseType) || type.DerivesFrom(baseType);

    public static bool DerivesOrImplementsAny(this ITypeSymbol type, ImmutableArray<KnownType> baseTypes) =>
        type.ImplementsAny(baseTypes) || type.DerivesFromAny(baseTypes);

    public static ITypeSymbol GetSymbolType(this ISymbol symbol) =>
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

    private static ITypeSymbol NullableTypeArgument(ITypeSymbol type) =>
        type is INamedTypeSymbol namedType && namedType.OriginalDefinition.Is(KnownType.System_Nullable_T)
            ? namedType.TypeArguments[0]
            : null;
}
