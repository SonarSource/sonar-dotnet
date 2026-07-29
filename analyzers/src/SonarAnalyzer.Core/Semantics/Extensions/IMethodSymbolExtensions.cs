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

    extension(IMethodSymbol method)
    {
        public bool IsExtensionOn(KnownType type)
        {
            if (method is { IsExtensionMethod: true })
            {
                var receiverType = method.MethodKind == MethodKind.Ordinary
                    ? method.Parameters.First().Type as INamedTypeSymbol
                    : method.ReceiverType as INamedTypeSymbol;
                return receiverType?.ConstructedFrom.Is(type) ?? false;
            }
            else
            {
                return false;
            }
        }

        public bool IsDestructor => method.MethodKind == MethodKind.Destructor;

        public bool Is(KnownType knownType, string name) =>
            method.ContainingType.Is(knownType) && method.Name == name;

        public bool IsAny(KnownType knownType, params string[] names) =>
            method.ContainingType.Is(knownType) && names.Contains(method.Name);

        public bool IsImplementingInterfaceMember(KnownType knownInterfaceType, string name) =>
            (method.Name == name && (method.Is(knownInterfaceType, name) || method.InterfaceMembers().Any(x => x.Is(knownInterfaceType, name))))
            || method.ExplicitInterfaceImplementations.Any(x => x.ContainingType.ConstructedFrom.Is(knownInterfaceType) && x.Name == name);

        /// <summary>
        /// Returns a value indicating whether the provided method symbol is a ASP.NET MVC
        /// controller method.
        /// </summary>
        public bool IsControllerActionMethod =>
            method is { MethodKind: MethodKind.Ordinary, IsStatic: false, EffectiveAccessibility: Accessibility.Public, TypeParameters.Length: 0, ContainingType.IsControllerType: true }
            && (method.OverriddenMethod is null
                || !method.OverriddenMethod.ContainingType.IsAny(KnownType.Microsoft_AspNetCore_Mvc_ControllerBase, KnownType.Microsoft_AspNetCore_Mvc_Controller))
            && !method.GetAttributes().Any(x => x.AttributeClass.IsAny(NonActionTypes))
            && method.Parameters.All(x => x.RefKind == RefKind.None);

        public Comparison ComparisonKind =>
            method?.MethodKind == MethodKind.UserDefinedOperator
                ? ComparisonKindFromOperatorName(method.Name)
                : Comparison.None;

        public bool IsTestMethod =>
            method.MethodKind.HasFlag(MethodKindEx.LocalFunction)
                ? method.IsXunitTestMethod()
                : method.AnyAttributeDerivesFromOrImplementsAny(KnownTestMethodAttributes);

        public bool IsIgnoredTestMethod =>
            method.HasTestIgnoreAttribute()
            || (method.FindXUnitTestAttribute() is { } testAttribute
                && (testAttribute.NamedArguments.Any(x => x.Key is "Skip" or "SkipExceptions" or "SkipType" or "SkipUnless" or "SkipWhen")
                    || (testAttribute.TryGetAttributeValue("Explicit", out bool explicitTest) && explicitTest)));

        public bool HasExpectedExceptionAttribute =>
            method.GetAttributes().Any(x =>
                x.AttributeClass.IsAny(KnownType.ExpectedExceptionAttributes)
                || x.AttributeClass.DerivesFrom(KnownType.Microsoft_VisualStudio_TestTools_UnitTesting_ExpectedExceptionBaseAttribute));

        public bool HasAssertionInAttribute =>
            !NoExpectedResultTestMethodReturnTypes.Any(method.ReturnType.Is)
            && method.GetAttributes().Any(IsAnyTestCaseAttributeWithExpectedResult);

        public bool IsMsTestOrNUnitTestIgnored => method.GetAttributes().Any(x => x.AttributeClass.IsAny(KnownType.IgnoreAttributes));

        /// <summary>
        /// Returns the <see cref="KnownType"/> that indicates the type of the test method or
        /// null if the method is not decorated with a known type.
        /// </summary>
        /// <remarks>We assume that a test is only marked with a single test attribute e.g.
        /// not both [Fact] and [Theory]. If there are multiple attributes only one will be
        /// returned.</remarks>
        public KnownType FirstTestMethodType => KnownTestMethodAttributes.FirstOrDefault(x => method.GetAttributes().Any(att => att.AttributeClass.DerivesFrom(x)));

        public bool IsExtension => method is { IsExtensionMethod: true } or { AssociatedExtensionImplementation: not null };

        /// <summary>
        /// Returns whether the method is a constructor in a MEF-exported type.
        /// MEF (Managed Extensibility Framework) instantiates types via reflection, so these constructors are not unused.
        /// </summary>
        public bool IsMefConstructor => method is { MethodKind: MethodKind.Constructor, ContainingType: INamedTypeSymbol { IsMefExportedType: true } };

        private AttributeData FindXUnitTestAttribute() =>
            method.GetAttributes().FirstOrDefault(x => x.AttributeClass.IsAny(KnownType.TestMethodAttributesOfxUnit));

        private bool HasTestIgnoreAttribute() =>
           method.GetAttributes().Any(x => x.AttributeClass.IsAny(KnownTestIgnoreAttributes));

        private bool IsXunitTestMethod() =>
            method.AnyAttributeDerivesFromAny(KnownType.TestMethodAttributesOfxUnit);

        private static bool IsAnyTestCaseAttributeWithExpectedResult(AttributeData a) =>
            IsTestAttributeWithExpectedResult(a)
            || a.AttributeClass.Is(KnownType.NUnit_Framework_TestCaseSourceAttribute);

        private static bool IsTestAttributeWithExpectedResult(AttributeData attribute) =>
            attribute.AttributeClass.IsAny(KnownType.NUnit_Framework_TestCaseAttribute, KnownType.NUnit_Framework_TestAttribute)
            && attribute.NamedArguments.Any(x => x.Key == "ExpectedResult");

        private static Comparison ComparisonKindFromOperatorName(string methodName) =>
            methodName switch
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
}
