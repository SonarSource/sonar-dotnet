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

using Comparison = SonarAnalyzer.Helpers.ComparisonKind;

namespace SonarAnalyzer.Helpers;

public static class MethodSymbolExtensions
{
    public static bool Is(this IMethodSymbol methodSymbol, KnownType knownType, string name) =>
        methodSymbol.ContainingType.Is(knownType) && methodSymbol.Name == name;

    public static bool IsAny(this IMethodSymbol methodSymbol, KnownType knownType, params string[] names) =>
        methodSymbol.ContainingType.Is(knownType) && names.Contains(methodSymbol.Name);

    public static bool IsImplementingInterfaceMember(this IMethodSymbol methodSymbol, KnownType knownInterfaceType, string name) =>
        methodSymbol.Name == name
        && (methodSymbol.Is(knownInterfaceType, name)
            || (methodSymbol.GetInterfaceMember() is { } implementedInterfaceMember && implementedInterfaceMember.Is(knownInterfaceType, name)));

    public static Comparison ComparisonKind(this IMethodSymbol method) =>
        method?.MethodKind == MethodKind.UserDefinedOperator
            ? ComparisonKind(method.Name)
            : Comparison.None;

    private static Comparison ComparisonKind(string method) =>
        method switch
        {
            "op_Equality" => Comparison.Equals,
            "op_Inequality" => Comparison.NotEquals,
            "op_LessThan" => Comparison.LessThan,
            "op_LessThanOrEqual" => Comparison.LessThanOrEqual,
            "op_GreaterThan" => Comparison.GreaterThan,
            "op_GreaterThanOrEqual" => Comparison.GreaterThanOrEqual,
            _ => Comparison.None,
        };
}
