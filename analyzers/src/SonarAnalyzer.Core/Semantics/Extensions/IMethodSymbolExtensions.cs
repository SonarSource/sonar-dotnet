/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

using Comparison = SonarAnalyzer.Core.Syntax.Utilities.ComparisonKind;

namespace SonarAnalyzer.Core.Semantics.Extensions;

public static class IMethodSymbolExtensions
{
    private static readonly ImmutableArray<KnownType> NonActionTypes = ImmutableArray.Create(KnownType.Microsoft_AspNetCore_Mvc_NonActionAttribute, KnownType.System_Web_Mvc_NonActionAttribute);

    private static readonly ImmutableArray<KnownType> KnownTestMethodAttributes = ImmutableArray.Create(
        [
            ..KnownType.TestMethodAttributesOfMSTest,
            ..KnownType.TestMethodAttributesOfNUnit,
            ..KnownType.TestMethodAttributesOfxUnit,
        ]);

    private static readonly ImmutableArray<KnownType> NoExpectedResultTestMethodReturnTypes = ImmutableArray.Create(
            KnownType.Void,
            KnownType.System_Threading_Tasks_Task);

    private static readonly ImmutableArray<KnownType> KnownTestIgnoreAttributes = ImmutableArray.Create(
           // Note: XUnit doesn't have a separate "Ignore" attribute. It has a "Skip" parameter
           // on the test attribute
           KnownType.Microsoft_VisualStudio_TestTools_UnitTesting_IgnoreAttribute,
           KnownType.NUnit_Framework_IgnoreAttribute);

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
        && (methodSymbol.Is(knownInterfaceType, name) || methodSymbol.InterfaceMembers().Any(x => x.Is(knownInterfaceType, name)));

    /// <summary>
    /// Returns a value indicating whether the provided method symbol is a ASP.NET MVC
    /// controller method.
    /// </summary>
    public static bool IsControllerActionMethod(this IMethodSymbol methodSymbol) =>
        methodSymbol is { MethodKind: MethodKind.Ordinary, IsStatic: false }
        && (methodSymbol.OverriddenMethod is null
            || !methodSymbol.OverriddenMethod.ContainingType.IsAny(KnownType.Microsoft_AspNetCore_Mvc_ControllerBase, KnownType.Microsoft_AspNetCore_Mvc_Controller))
        && methodSymbol.GetEffectiveAccessibility() == Accessibility.Public
        && !methodSymbol.GetAttributes().Any(x => x.AttributeClass.IsAny(NonActionTypes))
        && methodSymbol.TypeParameters.Length == 0
        && methodSymbol.Parameters.All(x => x.RefKind == RefKind.None)
        && methodSymbol.ContainingType.IsControllerType();

    public static Comparison ComparisonKind(this IMethodSymbol method) =>
        method?.MethodKind == MethodKind.UserDefinedOperator
            ? ComparisonKind(method.Name)
            : Comparison.None;

    public static bool IsTestMethod(this IMethodSymbol method) =>
        method.MethodKind.HasFlag(MethodKindEx.LocalFunction)
            ? method.IsXunitTestMethod()
            : method.AnyAttributeDerivesFromOrImplementsAny(KnownTestMethodAttributes);

    public static bool IsIgnoredTestMethod(this IMethodSymbol method) =>
        method.HasTestIgnoreAttribute()
        || (method.FindXUnitTestAttribute() is { } testAttribute
            && (testAttribute.NamedArguments.Any(x => x.Key is "Skip" or "SkipExceptions" or "SkipType" or "SkipUnless" or "SkipWhen")
                || (testAttribute.TryGetAttributeValue("Explicit", out bool explicitTest) && explicitTest)));

    public static bool HasExpectedExceptionAttribute(this IMethodSymbol method) =>
        method.GetAttributes().Any(x =>
            x.AttributeClass.IsAny(KnownType.ExpectedExceptionAttributes)
            || x.AttributeClass.DerivesFrom(KnownType.Microsoft_VisualStudio_TestTools_UnitTesting_ExpectedExceptionBaseAttribute));

    public static bool HasAssertionInAttribute(this IMethodSymbol method) =>
        !NoExpectedResultTestMethodReturnTypes.Any(method.ReturnType.Is)
        && method.GetAttributes().Any(IsAnyTestCaseAttributeWithExpectedResult);

    public static bool IsMsTestOrNUnitTestIgnored(this IMethodSymbol method) =>
        method.GetAttributes().Any(x => x.AttributeClass.IsAny(KnownType.IgnoreAttributes));

    /// <summary>
    /// Returns the <see cref="KnownType"/> that indicates the type of the test method or
    /// null if the method is not decorated with a known type.
    /// </summary>
    /// <remarks>We assume that a test is only marked with a single test attribute e.g.
    /// not both [Fact] and [Theory]. If there are multiple attributes only one will be
    /// returned.</remarks>
    public static KnownType FindFirstTestMethodType(this IMethodSymbol method) =>
        KnownTestMethodAttributes.FirstOrDefault(x => method.GetAttributes().Any(att => att.AttributeClass.DerivesFrom(x)));

    extension(IMethodSymbol method)
    {
        public bool IsExtension => method is { IsExtensionMethod: true } or { AssociatedExtensionImplementation: not null };
    }

    /// <summary>
    /// Returns whether the method is a constructor in a MEF-exported type.
    /// MEF (Managed Extensibility Framework) instantiates types via reflection, so these constructors are not unused.
    /// </summary>
    public static bool IsMefConstructor(this IMethodSymbol methodSymbol) =>
        methodSymbol is { MethodKind: MethodKind.Constructor, ContainingType: INamedTypeSymbol containingType }
        && containingType.IsMefExportedType();

    private static AttributeData FindXUnitTestAttribute(this IMethodSymbol method) =>
        method.GetAttributes().FirstOrDefault(x => x.AttributeClass.IsAny(KnownType.TestMethodAttributesOfxUnit));

    private static bool IsAnyTestCaseAttributeWithExpectedResult(AttributeData a) =>
        IsTestAttributeWithExpectedResult(a)
        || a.AttributeClass.Is(KnownType.NUnit_Framework_TestCaseSourceAttribute);

    private static bool HasTestIgnoreAttribute(this IMethodSymbol method) =>
       method.GetAttributes().Any(x => x.AttributeClass.IsAny(KnownTestIgnoreAttributes));

    private static bool IsTestAttributeWithExpectedResult(AttributeData attribute) =>
        attribute.AttributeClass.IsAny(KnownType.NUnit_Framework_TestCaseAttribute, KnownType.NUnit_Framework_TestAttribute)
        && attribute.NamedArguments.Any(x => x.Key == "ExpectedResult");

    private static bool IsXunitTestMethod(this IMethodSymbol methodSymbol) =>
        methodSymbol.AnyAttributeDerivesFromAny(KnownType.TestMethodAttributesOfxUnit);

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
