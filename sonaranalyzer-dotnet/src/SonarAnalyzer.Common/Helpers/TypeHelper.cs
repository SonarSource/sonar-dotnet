/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2017 SonarSource SA
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

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace SonarAnalyzer.Helpers
{
    internal static class TypeHelper
    {
        #region TypeKind

        public static bool IsInterface(this ITypeSymbol self)
        {
            return self != null && self.TypeKind == TypeKind.Interface;
        }

        public static bool IsClass(this ITypeSymbol self)
        {
            return self != null && self.TypeKind == TypeKind.Class;
        }

        public static bool IsStruct(this ITypeSymbol self)
        {
            return self != null && self.TypeKind == TypeKind.Struct;
        }

        public static bool IsClassOrStruct(this ITypeSymbol self)
        {
            return self.IsStruct() || self.IsClass();
        }

        public static bool Is(this ITypeSymbol self, TypeKind typeKind)
        {
            return self != null && self.TypeKind == typeKind;
        }

        #endregion

        #region TypeName

        private static bool IsMatch(ITypeSymbol typeSymbol, KnownType type)
        {
            return type.Matches(typeSymbol.SpecialType) || type.Matches(typeSymbol.ToDisplayString());
        }

        public static bool Is(this ITypeSymbol typeSymbol, KnownType type)
        {
            return typeSymbol != null && IsMatch(typeSymbol, type);
        }

        public static bool IsAny(this ITypeSymbol typeSymbol, ISet<KnownType> types)
        {
            return typeSymbol != null && types.Any(t => IsMatch(typeSymbol, t));
        }

        public static bool IsType(this IParameterSymbol parameter, KnownType type)
        {
            return parameter != null && parameter.Type.Is(type);
        }

        public static bool IsInType(this ISymbol symbol, KnownType type)
        {
            return symbol != null && symbol.ContainingType.Is(type);
        }

        public static bool IsInType(this ISymbol symbol, ITypeSymbol type)
        {
            return symbol?.ContainingType != null &&
                symbol.ContainingType.Equals(type);
        }

        public static bool IsInType(this ISymbol symbol, ISet<KnownType> types)
        {
            return symbol != null && symbol.ContainingType.IsAny(types);
        }

        #endregion

        public static bool Implements(this ITypeSymbol typeSymbol, KnownType type)
        {
            return typeSymbol != null &&
                typeSymbol.AllInterfaces.Any(symbol => symbol.ConstructedFrom.Is(type));
        }

        public static bool Implements(this ITypeSymbol typeSymbol, ITypeSymbol type)
        {
            return typeSymbol != null &&
                typeSymbol.AllInterfaces.Any(symbol => symbol.ConstructedFrom.Equals(type));
        }

        public static bool ImplementsAny(this ITypeSymbol typeSymbol, ISet<KnownType> types)
        {
            return typeSymbol != null &&
                typeSymbol.AllInterfaces.Any(symbol => symbol.ConstructedFrom.IsAny(types));
        }

        public static bool DerivesFrom(this ITypeSymbol typeSymbol, KnownType type)
        {
            var currentType = typeSymbol;
            while(currentType != null)
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

        public static bool DerivesFromAny(this ITypeSymbol typeSymbol, ISet<KnownType> baseTypes)
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

        public static bool DerivesOrImplements(this ITypeSymbol type, KnownType baseType)
        {
            return type.Implements(baseType) ||
                type.DerivesFrom(baseType);
        }

        public static bool DerivesOrImplements(this ITypeSymbol type, ITypeSymbol baseType)
        {
            return type.Implements(baseType) ||
                type.DerivesFrom(baseType);
        }

        public static bool DerivesOrImplementsAny(this ITypeSymbol type, ISet<KnownType> baseTypes)
        {
            return type.ImplementsAny(baseTypes) ||
                type.DerivesFromAny(baseTypes);
        }

        public static ITypeSymbol GetSymbolType(this ISymbol symbol)
        {
            var localSymbol = symbol as ILocalSymbol;
            if (localSymbol != null)
            {
                return localSymbol.Type;
            }

            var fieldSymbol = symbol as IFieldSymbol;
            if (fieldSymbol != null)
            {
                return fieldSymbol.Type;
            }

            var propertySymbol = symbol as IPropertySymbol;
            if (propertySymbol != null)
            {
                return propertySymbol.Type;
            }

            var parameterSymbol = symbol as IParameterSymbol;
            if (parameterSymbol != null)
            {
                return parameterSymbol.Type;
            }

            var aliasSymbol = symbol as IAliasSymbol;
            if (aliasSymbol != null)
            {
                return aliasSymbol.Target as ITypeSymbol;
            }

            return symbol as ITypeSymbol;
        }
    }
}
