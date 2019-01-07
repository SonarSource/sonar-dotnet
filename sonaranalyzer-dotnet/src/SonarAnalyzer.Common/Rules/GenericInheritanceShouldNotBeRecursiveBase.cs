/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class GenericInheritanceShouldNotBeRecursiveBase : SonarDiagnosticAnalyzer
    {
        protected const string DiagnosticId = "S3464";
        protected const string MessageFormat = "Refactor this {0} so that the generic inheritance chain is not recursive.";
        protected static IEnumerable<INamedTypeSymbol> GetBaseTypes(INamedTypeSymbol typeSymbol)
        {
            var interfaces = typeSymbol.Interfaces.Where(IsGenericType);
            return typeSymbol.IsClass()
                ? interfaces.Concat(new[] { typeSymbol.BaseType })
                : interfaces;
        }

        protected static bool HasRecursiveGenericSubstitution(INamedTypeSymbol typeSymbol, INamedTypeSymbol declaredType)
        {
            bool IsSameAsDeclaredType(INamedTypeSymbol type) =>
                type.OriginalDefinition.Equals(declaredType) && HasSubstitutedTypeArguments(type);

            bool ContainsRecursiveGenericSubstitution(IEnumerable<ITypeSymbol> types) =>
                types.OfType<INamedTypeSymbol>()
                    .Any(type => IsSameAsDeclaredType(type) || ContainsRecursiveGenericSubstitution(type.TypeArguments));

            return ContainsRecursiveGenericSubstitution(typeSymbol.TypeArguments);
        }

        protected static bool IsGenericType(INamedTypeSymbol type) =>
            type != null && type.IsGenericType;

        protected static bool HasSubstitutedTypeArguments(INamedTypeSymbol type) =>
            type.TypeArguments.OfType<INamedTypeSymbol>().Any();

        protected bool IsRecursiveInheritance(INamedTypeSymbol typeSymbol)
        {
            if (!IsGenericType(typeSymbol))
            {
                return false;
            }

            var baseTypes = GetBaseTypes(typeSymbol);

            return baseTypes.Any(t => IsGenericType(t) && HasRecursiveGenericSubstitution(t, typeSymbol));
        }
    }
}
