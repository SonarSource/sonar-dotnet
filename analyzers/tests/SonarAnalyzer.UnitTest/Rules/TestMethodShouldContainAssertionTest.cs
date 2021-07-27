/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Rules.CSharp;
using SonarAnalyzer.UnitTest.MetadataReferences;
using SonarAnalyzer.UnitTest.TestFramework;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class TestMethodShouldContainAssertionTest
    {
        private static class FluentAssertionVersions
        {
            public const string Ver1 = "1.6.0";
            public const string Ver4 = "4.19.4";
            public const string Ver5 = "5.9.0";
        }

        private static class MsTestVersions
        {
            public const string Ver1 = "1.1.11";
        }

        private static class NUnitVersions
        {
            public const string Ver3 = "3.11.0";
            public const string Ver25 = "2.5.7.10213";
            public const string Ver27 = "2.7.0";
        }

        private static class XUnitVersions
        {
            public const string Ver2 = "2.0.0";
        }

        [DataTestMethod]
        [DataRow(MsTestVersions.Ver1)]
        [DataRow(Constants.NuGetLatestVersion)]
        [TestCategory("Rule")]
        public void TestMethodShouldContainAssertion_MSTest(string testFwkVersion) =>
            Verifier.VerifyAnalyzer(new[] { @"TestCases\TestMethodShouldContainAssertion.MsTest.cs", @"TestCases\TestMethodShouldContainAssertion.MsTest.AnotherFile.cs" },
                                    new TestMethodShouldContainAssertion(),
                                    AdditionalTestReferences(NuGetMetadataReference.MSTestTestFramework(testFwkVersion)));

        [DataTestMethod]
        [DataRow(NUnitVersions.Ver3, Constants.NuGetLatestVersion, Constants.NuGetLatestVersion)]
        [DataRow(Constants.NuGetLatestVersion, FluentAssertionVersions.Ver5, Constants.NuGetLatestVersion)]
        [DataRow(Constants.NuGetLatestVersion, Constants.NuGetLatestVersion, Constants.NuGetLatestVersion)]
        [TestCategory("Rule")]
        public void TestMethodShouldContainAssertion_NUnit(string testFwkVersion, string fluentVersion, string nSubstituteVersion) =>
            Verifier.VerifyAnalyzer(@"TestCases\TestMethodShouldContainAssertion.NUnit.cs",
                new TestMethodShouldContainAssertion(),
                AdditionalTestReferences(NuGetMetadataReference.NUnit(testFwkVersion), fluentVersion, nSubstituteVersion));

        [DataTestMethod]
        [DataRow(NUnitVersions.Ver25)]
        [DataRow(NUnitVersions.Ver27)]
        [TestCategory("Rule")]
        public void TestMethodShouldContainAssertion_NUnit_V2Specific(string testFwkVersion) =>
            Verifier.VerifyNonConcurrentCSharpAnalyzer(@"
using System;
using NUnit.Framework;

[TestFixture]
public class Foo
{
    [TestCase]
    [ExpectedException(typeof(Exception))]
    public void TestCase4()
    {
        var x = System.IO.File.Open("""", System.IO.FileMode.Open);
    }

    [Theory]
    [ExpectedException(typeof(Exception))]
    public void Theory4()
    {
        var x = System.IO.File.Open("""", System.IO.FileMode.Open);
    }

    [TestCaseSource(""Foo"")]
    [ExpectedException(typeof(Exception))]
    public void TestCaseSource4()
    {
        var x = System.IO.File.Open("""", System.IO.FileMode.Open);
    }

    [Test]
    [ExpectedException(typeof(Exception))]
    public void Test4()
    {
        var x = System.IO.File.Open("""", System.IO.FileMode.Open);
    }
}",
                new TestMethodShouldContainAssertion(),
                AdditionalTestReferences(NuGetMetadataReference.NUnit(testFwkVersion)));

        [DataTestMethod]
        [DataRow(XUnitVersions.Ver2, Constants.NuGetLatestVersion, Constants.NuGetLatestVersion)]
        [DataRow(Constants.NuGetLatestVersion, Constants.NuGetLatestVersion, Constants.NuGetLatestVersion)]
        [TestCategory("Rule")]
        public void TestMethodShouldContainAssertion_Xunit(string testFwkVersion, string fluentVersion, string nSubstituteVersion) =>
            Verifier.VerifyAnalyzer(@"TestCases\TestMethodShouldContainAssertion.Xunit.cs",
                new TestMethodShouldContainAssertion(),
                AdditionalTestReferences(NuGetMetadataReference.XunitFramework(testFwkVersion), fluentVersion, nSubstituteVersion));

        [TestMethod]
        [TestCategory("Rule")]
        public void TestMethodShouldContainAssertion_Xunit_Legacy() =>
            Verifier.VerifyAnalyzer(@"TestCases\TestMethodShouldContainAssertion.Xunit.Legacy.cs",
                new TestMethodShouldContainAssertion(),
                AdditionalTestReferences(NuGetMetadataReference.XunitFrameworkV1));

        [DataTestMethod]
        [DataRow(NUnitVersions.Ver25, FluentAssertionVersions.Ver1)]
        [DataRow(NUnitVersions.Ver25, FluentAssertionVersions.Ver4)]
        [TestCategory("Rule")]
        public void TestMethodShouldContainAssertion_NUnit_FluentAssertionsLegacy(string testFwkVersion, string fluentVersion) =>
            Verifier.VerifyNonConcurrentCSharpAnalyzer(@"
using System;
using FluentAssertions;
using NUnit.Framework;

[TestFixture]
public class Foo
{
    [Test]
    public void Test1() // Noncompliant
    {
        var x = 42;
    }

    [Test]
    public void ShouldThrowTest()
    {
        Action act = () => { throw new Exception(); };
        act.ShouldThrow<Exception>();
    }

    [Test]
    public void ShouldNotThrowTest()
    {
        Action act = () => { throw new Exception(); };
        act.ShouldNotThrow<Exception>();
    }
}",
                new TestMethodShouldContainAssertion(),
                AdditionalTestReferences(NuGetMetadataReference.NUnit(testFwkVersion), fluentVersion));

        [TestMethod]
        [TestCategory("Rule")]
        public void TestMethodShouldContainAssertion_CustomAssertionMethod() =>
            Verifier.VerifyAnalyzer(@"TestCases\TestMethodShouldContainAssertion.Custom.cs",
                new TestMethodShouldContainAssertion(),
                NuGetMetadataReference.MSTestTestFramework(Constants.NuGetLatestVersion));

#if NET
        [TestMethod]
        [TestCategory("Rule")]
        public void TestMethodShouldContainAssertion_CSharp9() =>
            Verifier.VerifyAnalyzerFromCSharp9Library(@"TestCases\TestMethodShouldContainAssertion.CSharp9.cs",
                                                new TestMethodShouldContainAssertion(),
                                                NuGetMetadataReference.MSTestTestFrameworkV1
                                                    .Concat(NuGetMetadataReference.XunitFramework(Constants.NuGetLatestVersion))
                                                    .Concat(NuGetMetadataReference.NUnit(Constants.NuGetLatestVersion))
                                                    .ToArray());
#endif

        public static IEnumerable<MetadataReference> AdditionalTestReferences(IEnumerable<MetadataReference> testFrameworkReference,
                                                                              string fluentVersion = Constants.NuGetLatestVersion,
                                                                              string nSubstituteVersion = Constants.NuGetLatestVersion) =>
            testFrameworkReference
                .Concat(NuGetMetadataReference.FluentAssertions(fluentVersion))
                .Concat(NuGetMetadataReference.NSubstitute(nSubstituteVersion))
                .Concat(MetadataReferenceFacade.SystemXml)
                .Concat(MetadataReferenceFacade.SystemXmlLinq)
                .Concat(MetadataReferenceFacade.SystemThreadingTasks)
                .ToArray();
    }
}
