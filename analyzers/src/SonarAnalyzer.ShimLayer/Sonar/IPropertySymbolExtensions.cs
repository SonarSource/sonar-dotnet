/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace StyleCop.Analyzers.Lightup;

public static class IPropertySymbolExtensions
{
    private static readonly Func<IPropertySymbol, bool> IsRequiredAccessor;
    private static readonly Func<IPropertySymbol, IPropertySymbol> PartialDefinitionPartAccessor;
    private static readonly Func<IPropertySymbol, IPropertySymbol> PartialImplementationPartAccessor;
    private static readonly Func<IPropertySymbol, bool> IsPartialDefinitionAccessor;

    static IPropertySymbolExtensions()
    {
        IsRequiredAccessor = LightupHelpers.CreateSyntaxPropertyAccessor<IPropertySymbol, bool>(typeof(IPropertySymbol), nameof(IsRequired));
        IsPartialDefinitionAccessor = LightupHelpers.CreateSyntaxPropertyAccessor<IPropertySymbol, bool>(typeof(IPropertySymbol), nameof(IsPartialDefinition));
        PartialDefinitionPartAccessor = LightupHelpers.CreateSyntaxPropertyAccessor<IPropertySymbol, IPropertySymbol>(typeof(IPropertySymbol), nameof(PartialDefinitionPart));
        PartialImplementationPartAccessor = LightupHelpers.CreateSyntaxPropertyAccessor<IPropertySymbol, IPropertySymbol>(typeof(IPropertySymbol), nameof(PartialImplementationPart));
    }

    public static bool IsRequired(this IPropertySymbol propertySymbol) =>
        IsRequiredAccessor(propertySymbol);

    public static bool IsPartialDefinition(this IPropertySymbol propertySymbol) =>
        IsPartialDefinitionAccessor(propertySymbol);

    public static IPropertySymbol PartialDefinitionPart(this IPropertySymbol propertySymbol) =>
        PartialDefinitionPartAccessor(propertySymbol);

    public static IPropertySymbol PartialImplementationPart(this IPropertySymbol propertySymbol) =>
        PartialImplementationPartAccessor(propertySymbol);
}
