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

using Comparison = SonarAnalyzer.Core.Syntax.Utilities.ComparisonKind;

namespace SonarAnalyzer.Core.Semantics.Extensions;

public static class IMethodSymbolExtensions
{
    private static readonly ImmutableArray<KnownType> NonActionTypes = ImmutableArray.Create(KnownType.Microsoft_AspNetCore_Mvc_NonActionAttribute, KnownType.System_Web_Mvc_NonActionAttribute);

    private static readonly ImmutableArray<KnownType> KnownTestMethodAttributes = ImmutableArray.Create(
        KnownType.TestMethodAttributesOfMSTest
            .Concat(KnownType.TestMethodAttributesOfNUnit)
            .Concat(KnownType.TestMethodAttributesOfxUnit)
            .ToArray());

    private static readonly ImmutableArray<KnownType> KnownTestClassAttributes = ImmutableArray.Create(
            // xUnit does not have have attributes to identity test classes
            KnownType.Microsoft_VisualStudio_TestTools_UnitTesting_TestClassAttribute,
            KnownType.NUnit_Framework_TestFixtureAttribute);

    private static readonly ImmutableArray<KnownType> NoExpectedResultTestMethodReturnTypes = ImmutableArray.Create(
            KnownType.Void,
            KnownType.System_Threading_Tasks_Task);

    private static readonly ImmutableArray<KnownType> KnownIgnoreAttributes = ImmutableArray.Create(
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

    /// <summary>
    /// Returns whether the class has an attribute that marks the class
    /// as an MSTest or NUnit test class (xUnit doesn't have any such attributes).
    /// </summary>
    public static bool IsTestClass(this INamedTypeSymbol classSymbol) =>
        classSymbol.AnyAttributeDerivesFromAny(KnownTestClassAttributes);

    public static bool IsTestMethod(this IMethodSymbol method) =>
        method.MethodKind.HasFlag(MethodKindEx.LocalFunction)
            ? method.IsXunitTestMethod()
            : method.AnyAttributeDerivesFromOrImplementsAny(KnownTestMethodAttributes);

    public static bool IsIgnoredTestMethod(this IMethodSymbol method) =>
        method.HasIgnoredAttribute()
        || method.FindXUnitTestAttribute().NamedArguments.Any(x => x.Key == "Skip");

    public static bool HasExpectedExceptionAttribute(this IMethodSymbol method) =>
        method.GetAttributes().Any(x =>
            x.AttributeClass.IsAny(KnownType.ExpectedExceptionAttributes)
            || x.AttributeClass.DerivesFrom(KnownType.Microsoft_VisualStudio_TestTools_UnitTesting_ExpectedExceptionBaseAttribute));

    public static bool HasAssertionInAttribute(this IMethodSymbol method) =>
        !NoExpectedResultTestMethodReturnTypes.Any(method.ReturnType.Is)
        && method.GetAttributes().Any(IsAnyTestCaseAttributeWithExpectedResult);

    public static bool IsMsTestOrNUnitTestIgnored(this IMethodSymbol method) =>
        method.GetAttributes().Any(x => x.AttributeClass.IsAny(KnownType.IgnoreAttributes));

    public static AttributeData FindXUnitTestAttribute(this IMethodSymbol method) =>
        method.GetAttributes().FirstOrDefault(x =>
            x.AttributeClass.Is(KnownType.Xunit_FactAttribute)
            || x.AttributeClass.Is(KnownType.Xunit_TheoryAttribute)
            || x.AttributeClass.Is(KnownType.LegacyXunit_TheoryAttribute));

    /// <summary>
    /// Returns the <see cref="KnownType"/> that indicates the type of the test method or
    /// null if the method is not decorated with a known type.
    /// </summary>
    /// <remarks>We assume that a test is only marked with a single test attribute e.g.
    /// not both [Fact] and [Theory]. If there are multiple attributes only one will be
    /// returned.</remarks>
    public static KnownType FindFirstTestMethodType(this IMethodSymbol method) =>
        KnownTestMethodAttributes.FirstOrDefault(x => method.GetAttributes().Any(att => att.AttributeClass.DerivesFrom(x)));

    private static bool IsAnyTestCaseAttributeWithExpectedResult(AttributeData a) =>
        IsTestAttributeWithExpectedResult(a)
        || a.AttributeClass.Is(KnownType.NUnit_Framework_TestCaseSourceAttribute);

    private static bool HasIgnoredAttribute(this IMethodSymbol method) =>
       method.GetAttributes().Any(x => x.AttributeClass.IsAny(KnownIgnoreAttributes));

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
