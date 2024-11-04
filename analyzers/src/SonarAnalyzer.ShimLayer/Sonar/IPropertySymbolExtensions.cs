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
