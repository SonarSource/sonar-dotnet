/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;

// Note: useful comparison of the differing syntax across unit test frameworks
// at https://xunit.github.io/docs/comparisons

namespace SonarAnalyzer.Helpers
{
    internal static class UnitTestHelper
    {
        public static readonly ImmutableArray<KnownType> KnownTestMethodAttributes_MSTest =
            ImmutableArray.Create(
                KnownType.Microsoft_VisualStudio_TestTools_UnitTesting_TestMethodAttribute,
                KnownType.Microsoft_VisualStudio_TestTools_UnitTesting_DataTestMethodAttribute
            );

        public static readonly ImmutableArray<KnownType> KnownTestMethodAttributes_NUnit =
            ImmutableArray.Create(
                KnownType.NUnit_Framework_TestAttribute,
                KnownType.NUnit_Framework_TestCaseAttribute,
                KnownType.NUnit_Framework_TestCaseSourceAttribute,
                KnownType.NUnit_Framework_TheoryAttribute
            );

        public static readonly ImmutableArray<KnownType> KnownTestMethodAttributes_xUnit =
            ImmutableArray.Create(
                KnownType.Xunit_FactAttribute,
                KnownType.Xunit_TheoryAttribute,
                KnownType.LegacyXunit_TheoryAttribute
            );

        public static readonly ImmutableArray<KnownType> KnownExpectedExceptionAttributes =
            ImmutableArray.Create(
                // Note: XUnit doesn't have a exception attribute
                KnownType.Microsoft_VisualStudio_TestTools_UnitTesting_ExpectedExceptionAttribute,
                KnownType.NUnit_Framework_ExpectedExceptionAttribute
            );

        public static readonly ImmutableArray<KnownType> KnownIgnoreAttributes =
            ImmutableArray.Create(
                // Note: XUnit doesn't have a separate "Ignore" attribute. It has a "Skip" parameter
                // on the test attribute
                KnownType.Microsoft_VisualStudio_TestTools_UnitTesting_IgnoreAttribute,
                KnownType.NUnit_Framework_IgnoreAttribute
            );

        private static readonly ImmutableArray<KnownType> KnownTestMethodAttributes =
            ImmutableArray.Create(
                KnownTestMethodAttributes_MSTest
                .Concat(KnownTestMethodAttributes_NUnit)
                .Concat(KnownTestMethodAttributes_xUnit)
                .ToArray()
            );

        /// <summary>
        /// List of partial names that are assumed to indicate an assertion method
        /// </summary>
        public static readonly ImmutableArray<string> KnownAssertionMethodParts =
            ImmutableArray.Create(
                "ASSERT",
                "CHECK",
                "EXPECT",
                "MUST",
                "SHOULD",
                "VERIFY",
                "VALIDATE"
            );

        private static readonly ImmutableArray<KnownType> KnownTestClassAttributes =
            ImmutableArray.Create(
                // xUnit does not have have attributes to identity test classes
                KnownType.Microsoft_VisualStudio_TestTools_UnitTesting_TestClassAttribute,
                KnownType.NUnit_Framework_TestFixtureAttribute
            );

        /// <summary>
        /// Returns whether the class has an attribute that marks the class
        /// as an MSTest or NUnit test class (xUnit doesn't have any such attributes)
        /// </summary>
        public static bool IsTestClass(this INamedTypeSymbol classSymbol) =>
            classSymbol.GetAttributes(KnownTestClassAttributes).Any();

        public static bool IsTestMethod(this IMethodSymbol method) =>
            method.GetAttributes(KnownTestMethodAttributes).Any();

        public static bool HasExpectedExceptionAttribute(this IMethodSymbol method) =>
            method.GetAttributes().Any(a => a.AttributeClass.IsAny(KnownExpectedExceptionAttributes));

        public static bool IsMsTestOrNUnitTestIgnored(this IMethodSymbol method)
        {
            var attributes = method.GetAttributes();
            if (attributes.Any(a => a.AttributeClass.IsAny(KnownIgnoreAttributes)))
            {
                return true;
            }
            return false;
        }

        public static AttributeData FindXUnitTestAttribute(this IMethodSymbol method) =>
            method.GetAttributes().FirstOrDefault(a =>
                    a.AttributeClass.Is(KnownType.Xunit_FactAttribute) ||
                    a.AttributeClass.Is(KnownType.Xunit_TheoryAttribute) ||
                    a.AttributeClass.Is(KnownType.LegacyXunit_TheoryAttribute));

        /// <summary>
        /// Returns the <see cref="KnownType"/> that indicates the type of the test method or
        /// null if the method is not decorated with a known type.
        /// </summary>
        /// <remarks>We assume that a test is only marked with a single test attribute e.g.
        /// not both [Fact] and [Theory]. If there are multiple attributes only one will be
        /// returned.</remarks>
        public static KnownType FindFirstTestMethodType(this IMethodSymbol method) =>
            KnownTestMethodAttributes.FirstOrDefault(known =>
                    method.GetAttributes().Any(att => att.AttributeClass.Is(known)));
    }
}
