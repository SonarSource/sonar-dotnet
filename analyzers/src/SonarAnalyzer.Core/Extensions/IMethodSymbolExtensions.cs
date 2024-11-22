/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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

using Comparison = SonarAnalyzer.Helpers.ComparisonKind;

namespace SonarAnalyzer.Core.Extensions;

public static class IMethodSymbolExtensions
{
    public static bool IsExtensionOn(this IMethodSymbol methodSymbol, KnownType type)
    {
        if (methodSymbol is { IsExtensionMethod: true })
        {
            var receiverType = methodSymbol.MethodKind == MethodKind.Ordinary
                ? methodSymbol.Parameters.First().Type as INamedTypeSymbol
                : methodSymbol.ReceiverType as INamedTypeSymbol;
            return receiverType?.ConstructedFrom.Is(type) ?? false;
        }
        else
        {
            return false;
        }
    }

    public static bool IsDestructor(this IMethodSymbol method) =>
        method.MethodKind == MethodKind.Destructor;

    public static bool IsAnyAttributeInOverridingChain(this IMethodSymbol method) =>
        method.IsAnyAttributeInOverridingChain(x => x.OverriddenMethod);

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
