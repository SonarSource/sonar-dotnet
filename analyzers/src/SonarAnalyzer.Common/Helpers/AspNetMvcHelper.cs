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
    public static class AspNetMvcHelper
    {
        public static readonly ImmutableArray<KnownType> RouteTemplateAttributes =
            ImmutableArray.Create(
                KnownType.Microsoft_AspNetCore_Mvc_Routing_HttpMethodAttribute,
                KnownType.Microsoft_AspNetCore_Mvc_RouteAttribute,
                KnownType.System_Web_Mvc_RouteAttribute);

        private static readonly ImmutableArray<KnownType> ControllerTypes =
            ImmutableArray.Create(
                KnownType.Microsoft_AspNetCore_Mvc_ControllerBase,
                KnownType.System_Web_Mvc_Controller);

        private static readonly ImmutableArray<KnownType> NonActionTypes =
            ImmutableArray.Create(
                KnownType.Microsoft_AspNetCore_Mvc_NonActionAttribute,
                KnownType.System_Web_Mvc_NonActionAttribute);

        private static readonly ImmutableArray<KnownType> NonControllerAttributeTypes =
            ImmutableArray.Create(KnownType.Microsoft_AspNetCore_Mvc_NonControllerAttribute);

        private static readonly ImmutableArray<KnownType> ControllerAttributeTypes =
            ImmutableArray.Create(KnownType.Microsoft_AspNetCore_Mvc_ControllerAttribute);

        /// <summary>
        /// Returns a value indicating whether the provided method symbol is a ASP.NET MVC
        /// controller method.
        /// </summary>
        public static bool IsControllerMethod(this IMethodSymbol methodSymbol) =>
            methodSymbol is { MethodKind: MethodKind.Ordinary, IsStatic: false }
            && (methodSymbol.OverriddenMethod == null || !methodSymbol.OverriddenMethod.ContainingType.Is(KnownType.Microsoft_AspNetCore_Mvc_ControllerBase))
            && methodSymbol.GetEffectiveAccessibility() == Accessibility.Public
            && !methodSymbol.GetAttributes().Any(d => d.AttributeClass.IsAny(NonActionTypes))
            && methodSymbol.TypeParameters.Length == 0
            && methodSymbol.Parameters.All(x => x.RefKind == RefKind.None)
            && IsControllerType(methodSymbol.ContainingType);

        /// <summary>
        /// Returns a value indicating whether the provided type symbol is a ASP.NET MVC
        /// controller.
        /// </summary>
        public static bool IsControllerType(this INamedTypeSymbol namedType) =>
            namedType != null
            && namedType.ContainingSymbol is not INamedTypeSymbol
            && (namedType.DerivesFromAny(ControllerTypes)
                || namedType.GetAttributes(ControllerAttributeTypes).Any())
            && !namedType.GetAttributes(NonControllerAttributeTypes).Any();

        public static bool ReferencesControllers(this Compilation compilation) =>
            compilation.GetTypeByMetadataName(KnownType.System_Web_Mvc_Controller) is not null
            || compilation.GetTypeByMetadataName(KnownType.Microsoft_AspNetCore_Mvc_Controller) is not null
            || compilation.GetTypeByMetadataName(KnownType.Microsoft_AspNetCore_Mvc_ControllerBase) is not null;
    }
}
