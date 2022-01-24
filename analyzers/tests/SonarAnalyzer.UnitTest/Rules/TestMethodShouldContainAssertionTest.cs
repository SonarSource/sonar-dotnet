/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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
using Microsoft.CodeAnalysis.CSharp;
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

        private readonly VerifierBuilder builder = new VerifierBuilder<TestMethodShouldContainAssertion>();

        [DataTestMethod]
        [DataRow(MsTestVersions.Ver1)]
        [DataRow(Constants.NuGetLatestVersion)]
        public void TestMethodShouldContainAssertion_MSTest(string testFwkVersion) =>
            builder.AddPaths("TestMethodShouldContainAssertion.MsTest.cs", "TestMethodShouldContainAssertion.MsTest.AnotherFile.cs" )
                .AddReferences(AdditionalTestReferences(NuGetMetadataReference.MSTestTestFramework(testFwkVersion)))
                .Verify();

        [DataTestMethod]
        [DataRow(NUnitVersions.Ver3, Constants.NuGetLatestVersion, Constants.NuGetLatestVersion)]
        [DataRow(Constants.NuGetLatestVersion, FluentAssertionVersions.Ver5, Constants.NuGetLatestVersion)]
        [DataRow(Constants.NuGetLatestVersion, Constants.NuGetLatestVersion, Constants.NuGetLatestVersion)]
        public void TestMethodShouldContainAssertion_NUnit(string testFwkVersion, string fluentVersion, string nSubstituteVersion) =>
            builder.AddPaths("TestMethodShouldContainAssertion.NUnit.cs")
                .AddReferences(AdditionalTestReferences(NuGetMetadataReference.NUnit(testFwkVersion), fluentVersion, nSubstituteVersion))
                .Verify();

        [DataTestMethod]
        [DataRow(NUnitVersions.Ver25)]
        [DataRow(NUnitVersions.Ver27)]
        public void TestMethodShouldContainAssertion_NUnit_V2Specific(string testFwkVersion) =>
            builder.AddSnippet(@"
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
}")
            .AddReferences(AdditionalTestReferences(NuGetMetadataReference.NUnit(testFwkVersion)))
            .Verify();

        [DataTestMethod]
        [DataRow(XUnitVersions.Ver2, Constants.NuGetLatestVersion, Constants.NuGetLatestVersion)]
        [DataRow(Constants.NuGetLatestVersion, Constants.NuGetLatestVersion, Constants.NuGetLatestVersion)]
        public void TestMethodShouldContainAssertion_Xunit(string testFwkVersion, string fluentVersion, string nSubstituteVersion) =>
            builder.AddPaths("TestMethodShouldContainAssertion.Xunit.cs")
                .AddReferences(AdditionalTestReferences(NuGetMetadataReference.XunitFramework(testFwkVersion), fluentVersion, nSubstituteVersion))
                .Verify();

        [TestMethod]
        public void TestMethodShouldContainAssertion_Xunit_Legacy() =>
            builder.AddPaths("TestMethodShouldContainAssertion.Xunit.Legacy.cs")
                .AddReferences(AdditionalTestReferences(NuGetMetadataReference.XunitFrameworkV1))
                .Verify();


        [DataTestMethod]
        [DataRow(NUnitVersions.Ver25, FluentAssertionVersions.Ver1)]
        [DataRow(NUnitVersions.Ver25, FluentAssertionVersions.Ver4)]
        public void TestMethodShouldContainAssertion_NUnit_FluentAssertionsLegacy(string testFwkVersion, string fluentVersion) =>
            builder.AddSnippet(@"
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
}")
            .AddReferences(AdditionalTestReferences(NuGetMetadataReference.NUnit(testFwkVersion), fluentVersion))
            .Verify();

        [TestMethod]
        public void TestMethodShouldContainAssertion_NUnit_NFluentLegacy() =>
           builder.AddSnippet(@"
using System;
using NFluent;
using NUnit.Framework;

[TestFixture]
public class Foo
{
    [Test]
    public void Test1()
    {
        throw new NFluent.FluentCheckException(""You failed me!"");
    }
}")
            .AddReferences(AdditionalTestReferences(NuGetMetadataReference.NUnit(NUnitVersions.Ver25), nFluentVersion: "1.3.1"))
            .Verify();

        [TestMethod]
        public void TestMethodShouldContainAssertion_CustomAssertionMethod() =>
            builder.AddPaths("TestMethodShouldContainAssertion.Custom.cs")
                .AddReferences(NuGetMetadataReference.MSTestTestFramework(Constants.NuGetLatestVersion))
                .Verify();

#if NET
        [TestMethod]
        public void TestMethodShouldContainAssertion_CSharp9() =>
            builder.AddPaths("TestMethodShouldContainAssertion.CSharp9.cs")
                .AddReferences(NuGetMetadataReference.MSTestTestFrameworkV1
                    .Concat(NuGetMetadataReference.XunitFramework(Constants.NuGetLatestVersion))
                    .Concat(NuGetMetadataReference.NUnit(Constants.NuGetLatestVersion)))
                .WithLanguageVersion(LanguageVersion.CSharp9)
                .Verify();
#endif

        public static IEnumerable<MetadataReference> AdditionalTestReferences(IEnumerable<MetadataReference> testFrameworkReference,
                                                                              string fluentVersion = Constants.NuGetLatestVersion,
                                                                              string nSubstituteVersion = Constants.NuGetLatestVersion,
                                                                              string nFluentVersion = Constants.NuGetLatestVersion,
                                                                              string shouldlyVersion = Constants.NuGetLatestVersion) =>
            testFrameworkReference
                .Concat(NuGetMetadataReference.FluentAssertions(fluentVersion))
                .Concat(NuGetMetadataReference.NSubstitute(nSubstituteVersion))
                .Concat(NuGetMetadataReference.NFluent(nFluentVersion))
                .Concat(NuGetMetadataReference.Shouldly(shouldlyVersion))
                .Concat(MetadataReferenceFacade.SystemData)
                .Concat(MetadataReferenceFacade.SystemXml)
                .Concat(MetadataReferenceFacade.SystemXmlLinq)
                .Concat(MetadataReferenceFacade.SystemThreadingTasks)
                .Concat(MetadataReferenceFacade.SystemNetHttp)
                .ToArray();
    }
}
