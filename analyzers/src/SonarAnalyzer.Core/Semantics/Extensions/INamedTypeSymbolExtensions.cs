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

public static class INamedTypeSymbolExtensions
{
    private static readonly ImmutableArray<KnownType> ControllerTypes = ImmutableArray.Create(KnownType.Microsoft_AspNetCore_Mvc_ControllerBase, KnownType.System_Web_Mvc_Controller);
    private static readonly ImmutableArray<KnownType> ControllerAttributeTypes = ImmutableArray.Create(KnownType.Microsoft_AspNetCore_Mvc_ControllerAttribute);

    private static readonly ImmutableArray<KnownType> KnownTestClassAttributes = ImmutableArray.Create(
        // xUnit does not have have attributes to identity test classes
        KnownType.Microsoft_VisualStudio_TestTools_UnitTesting_TestClassAttribute,
        KnownType.NUnit_Framework_TestFixtureAttribute);

    private static readonly ImmutableArray<KnownType> NonControllerAttributeTypes = ImmutableArray.Create(KnownType.Microsoft_AspNetCore_Mvc_NonControllerAttribute);

    extension(INamedTypeSymbol symbol)
    {
        public bool IsTopLevelProgram =>
            TopLevelStatements.ProgramClassImplicitName.Contains(symbol.Name)
            && symbol.ContainingNamespace.IsGlobalNamespace
            && symbol.GetMembers(TopLevelStatements.MainMethodImplicitName).Any();

        public IEnumerable<INamedTypeSymbol> AllNamedTypes
        {
            get
            {
                if (symbol is null)
                {
                    yield break;
                }

                yield return symbol;

                foreach (var nestedType in symbol.GetTypeMembers().SelectMany(x => x.AllNamedTypes))
                {
                    yield return nestedType;
                }
            }
        }

        /// <summary>
        /// Whether the provided type symbol is a ASP.NET MVC controller.
        /// </summary>
        public bool IsControllerType =>
            symbol is { ContainingSymbol: not INamedTypeSymbol }
            && (symbol.DerivesFromAny(ControllerTypes)
                || symbol.GetAttributes(ControllerAttributeTypes).Any())
            && !symbol.GetAttributes(NonControllerAttributeTypes).Any();

        /// <summary>
        /// Whether the provided type symbol is an ASP.NET Core API controller.
        /// Considers as API controllers also controllers deriving from ControllerBase but not Controller.
        /// </summary>
        public bool IsCoreApiController =>
            symbol.IsControllerType
            && (symbol.AttributesWithInherited.Any(x => x.AttributeClass.DerivesFrom(KnownType.Microsoft_AspNetCore_Mvc_ApiControllerAttribute))
                || (symbol.DerivesFrom(KnownType.Microsoft_AspNetCore_Mvc_ControllerBase) && !symbol.DerivesFrom(KnownType.Microsoft_AspNetCore_Mvc_Controller)));

        /// <summary>
        /// Returns whether the class has an attribute that marks the class
        /// as an MSTest or NUnit test class (xUnit doesn't have any such attributes).
        /// </summary>
        public bool IsTestClass => symbol.AnyAttributeDerivesFromAny(KnownTestClassAttributes);

        /// <summary>
        /// Returns whether the type is exported via MEF (Managed Extensibility Framework).
        /// Checks for [Export] attributes on the type itself or [InheritedExport] on base types/interfaces.
        /// Supports both MEF1 (System.ComponentModel.Composition) and MEF2 (System.Composition).
        /// </summary>
        public bool IsMefExportedType =>
            symbol is not null
            && (symbol.AnyAttributeDerivesFrom(KnownType.System_ComponentModel_Composition_ExportAttribute)
                || symbol.AnyAttributeDerivesFrom(KnownType.System_Composition_ExportAttribute)
                || symbol.SelfBaseTypesAndInterfaces.Any(x => x.AnyAttributeDerivesFrom(KnownType.System_ComponentModel_Composition_InheritedExportAttribute)));

        /// <summary>
        /// Returns the type itself, all base types, and all implemented interfaces.
        /// This is useful for checking inherited attributes across the full type hierarchy.
        /// </summary>
        public IEnumerable<INamedTypeSymbol> SelfBaseTypesAndInterfaces => symbol?.SelfAndBaseTypes.Union(symbol.AllInterfaces) ?? [];
    }
}
