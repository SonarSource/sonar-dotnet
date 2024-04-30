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

namespace SonarAnalyzer.Extensions;

internal static class IPropertySymbolExtensions
{
    public static bool IsAnyAttributeInOverridingChain(IPropertySymbol propertySymbol) =>
        IsAnyAttributeInOverridingChain(propertySymbol, property => property.OverriddenProperty);

    public static bool IsAnyAttributeInOverridingChain(IMethodSymbol methodSymbol) =>
        IsAnyAttributeInOverridingChain(methodSymbol, method => method.OverriddenMethod);

    private static bool IsAnyAttributeInOverridingChain<TSymbol>(TSymbol symbol, Func<TSymbol, TSymbol> getOverriddenMember)
        where TSymbol : class, ISymbol
    {
        var currentSymbol = symbol;
        while (currentSymbol != null)
        {
            if (currentSymbol.GetAttributes().Any())
            {
                return true;
            }

            if (!currentSymbol.IsOverride)
            {
                return false;
            }

            currentSymbol = getOverriddenMember(currentSymbol);
        }

        return false;
    }
}
