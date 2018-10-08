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

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.ControlFlowGraph.CSharp
{
    public static class TaintAnalysisEntryPointDetector
    {
        private static readonly ImmutableArray<KnownType> controllerTypes =
            ImmutableArray.Create(
                KnownType.Microsoft_AspNetCore_Mvc_ControllerBase,
                KnownType.System_Web_Mvc_Controller
            );

        private static readonly ImmutableArray<KnownType> nonControllerAttributeTypes =
            ImmutableArray.Create(KnownType.Microsoft_AspNetCore_Mvc_NonControllerAttribute);

        private static readonly ImmutableArray<KnownType> controllerAttributeTypes =
            ImmutableArray.Create(KnownType.Microsoft_AspNetCore_Mvc_ControllerAttribute);

        public static bool IsEntryPoint(IMethodSymbol methodSymbol) =>
            methodSymbol.GetEffectiveAccessibility() == Accessibility.Public &&
            IsControllerType(methodSymbol?.ContainingType);

        private static bool IsControllerType(INamedTypeSymbol containingType) =>
            containingType != null &&
            (containingType.DerivesFromAny(controllerTypes)
                || containingType.GetAttributes(controllerAttributeTypes).Any()) &&
            !containingType.GetAttributes(nonControllerAttributeTypes).Any();
    }
}
