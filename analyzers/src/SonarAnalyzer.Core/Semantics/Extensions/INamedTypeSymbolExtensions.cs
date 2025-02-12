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

namespace SonarAnalyzer.Core.Semantics.Extensions;

public static class INamedTypeSymbolExtensions
{
    private static readonly ImmutableArray<KnownType> ControllerTypes = ImmutableArray.Create(KnownType.Microsoft_AspNetCore_Mvc_ControllerBase, KnownType.System_Web_Mvc_Controller);
    private static readonly ImmutableArray<KnownType> ControllerAttributeTypes = ImmutableArray.Create(KnownType.Microsoft_AspNetCore_Mvc_ControllerAttribute);
    private static readonly ImmutableArray<KnownType> NonControllerAttributeTypes = ImmutableArray.Create(KnownType.Microsoft_AspNetCore_Mvc_NonControllerAttribute);

    public static bool IsTopLevelProgram(this INamedTypeSymbol symbol) =>
        TopLevelStatements.ProgramClassImplicitName.Contains(symbol.Name)
        && symbol.ContainingNamespace.IsGlobalNamespace
        && symbol.GetMembers(TopLevelStatements.MainMethodImplicitName).Any();

    public static IEnumerable<INamedTypeSymbol> GetAllNamedTypes(this INamedTypeSymbol type)
    {
        if (type is null)
        {
            yield break;
        }

        yield return type;

        foreach (var nestedType in type.GetTypeMembers().SelectMany(GetAllNamedTypes))
        {
            yield return nestedType;
        }
    }

    /// <summary>
    /// Whether the provided type symbol is a ASP.NET MVC controller.
    /// </summary>
    public static bool IsControllerType(this INamedTypeSymbol namedType) =>
        namedType is not null
        && namedType.ContainingSymbol is not INamedTypeSymbol
        && (namedType.DerivesFromAny(ControllerTypes)
            || namedType.GetAttributes(ControllerAttributeTypes).Any())
        && !namedType.GetAttributes(NonControllerAttributeTypes).Any();

    /// <summary>
    /// Whether the provided type symbol is an ASP.NET Core API controller.
    /// Considers as API controllers also controllers deriving from ControllerBase but not Controller.
    /// </summary>
    public static bool IsCoreApiController(this INamedTypeSymbol namedType) =>
        namedType.IsControllerType()
        && (namedType.GetAttributesWithInherited().Any(x => x.AttributeClass.DerivesFrom(KnownType.Microsoft_AspNetCore_Mvc_ApiControllerAttribute))
            || (namedType.DerivesFrom(KnownType.Microsoft_AspNetCore_Mvc_ControllerBase) && !namedType.DerivesFrom(KnownType.Microsoft_AspNetCore_Mvc_Controller)));
}
