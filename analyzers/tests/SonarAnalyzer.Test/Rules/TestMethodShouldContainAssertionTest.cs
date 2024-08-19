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

using SonarAnalyzer.Rules.CSharp;
using SonarAnalyzer.Test.Common;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class TestMethodShouldContainAssertionTest
{
    private const string Latest = Constants.NuGetLatestVersion; // Rename only

    private readonly VerifierBuilder builder = new VerifierBuilder<TestMethodShouldContainAssertion>();

    [DataTestMethod]
    [DataRow(MsTestVersions.Ver1)]
    [DataRow(Latest)]
    public void TestMethodShouldContainAssertion_MSTest(string testFwkVersion) =>
        WithTestReferences(NuGetMetadataReference.MSTestTestFramework(testFwkVersion))
            .AddPaths("TestMethodShouldContainAssertion.MsTest.cs", "TestMethodShouldContainAssertion.MsTest.AnotherFile.cs")
            .Verify();

    [DataTestMethod]
    [DataRow(NUnitVersions.Ver3, Latest, Latest)]
    [DataRow(NUnitVersions.Ver3Latest, FluentAssertionVersions.Ver5, Latest)] // Breaking changes in NUnit 4.0 would fail the test https://github.com/SonarSource/sonar-dotnet/issues/8409
    [DataRow(NUnitVersions.Ver3Latest, Latest, Latest)] // Breaking changes in NUnit 4.0 would fail the test https://github.com/SonarSource/sonar-dotnet/issues/8409
    public void TestMethodShouldContainAssertion_NUnit(string testFwkVersion, string fluentVersion, string nSubstituteVersion) =>
        WithTestReferences(NuGetMetadataReference.NUnit(testFwkVersion), fluentVersion, nSubstituteVersion).AddPaths("TestMethodShouldContainAssertion.NUnit.cs").Verify();

    [DataTestMethod]
    [DataRow(NUnitVersions.Ver25)]
    [DataRow(NUnitVersions.Ver27)]
    public void TestMethodShouldContainAssertion_NUnit_V2Specific(string testFwkVersion) =>
        WithTestReferences(NuGetMetadataReference.NUnit(testFwkVersion)).AddSnippet("""
            using System;
            using NUnit.Framework;

            [TestFixture]
            public class Foo
            {
                [TestCase]
                [ExpectedException(typeof(Exception))]
                public void TestCase4()
                {
                    var x = System.IO.File.Open("", System.IO.FileMode.Open);
                }

                [Theory]
                [ExpectedException(typeof(Exception))]
                public void Theory4()
                {
                    var x = System.IO.File.Open("", System.IO.FileMode.Open);
                }

                [TestCaseSource("Foo")]
                [ExpectedException(typeof(Exception))]
                public void TestCaseSource4()
                {
                    var x = System.IO.File.Open("", System.IO.FileMode.Open);
                }

                [Test]
                [ExpectedException(typeof(Exception))]
                public void Test4()
                {
                    var x = System.IO.File.Open("", System.IO.FileMode.Open);
                }
            }
            """).VerifyNoIssues();

    [DataTestMethod]
    [DataRow(XUnitVersions.Ver2, Latest, Latest)]
    [DataRow(XUnitVersions.Ver253, Latest, Latest)]
    public void TestMethodShouldContainAssertion_Xunit(string testFwkVersion, string fluentVersion, string nSubstituteVersion) =>
        WithTestReferences(NuGetMetadataReference.XunitFramework(testFwkVersion), fluentVersion, nSubstituteVersion).AddPaths("TestMethodShouldContainAssertion.Xunit.cs").Verify();

    [TestMethod]
    public void TestMethodShouldContainAssertion_Xunit_Legacy() =>
        WithTestReferences(NuGetMetadataReference.XunitFrameworkV1).AddPaths("TestMethodShouldContainAssertion.Xunit.Legacy.cs").Verify();

    [DataTestMethod]
    [DataRow(NUnitVersions.Ver25, FluentAssertionVersions.Ver1)]
    [DataRow(NUnitVersions.Ver25, FluentAssertionVersions.Ver4)]
    public void TestMethodShouldContainAssertion_NUnit_FluentAssertionsLegacy(string testFwkVersion, string fluentVersion) =>
        WithTestReferences(NuGetMetadataReference.NUnit(testFwkVersion), fluentVersion).AddSnippet("""
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
            }
            """).Verify();

    [TestMethod]
    public void TestMethodShouldContainAssertion_NUnit_NFluentLegacy() =>
       WithTestReferences(NuGetMetadataReference.NUnit(NUnitVersions.Ver25), nFluentVersion: "1.3.1").AddSnippet("""
           using System;
           using NFluent;
           using NUnit.Framework;

           [TestFixture]
           public class Foo
           {
               [Test]
               public void Test1()
               {
                   throw new NFluent.FluentCheckException("You failed me!");
               }
           }
           """).VerifyNoIssues();

    [TestMethod]
    public void TestMethodShouldContainAssertion_Moq() =>
        WithTestReferences(NuGetMetadataReference.MSTestTestFramework(Latest)).AddPaths("TestMethodShouldContainAssertion.Moq.cs").Verify();

    [TestMethod]
    public void TestMethodShouldContainAssertion_CustomAssertionMethod() =>
        builder.AddPaths("TestMethodShouldContainAssertion.Custom.cs").AddReferences(NuGetMetadataReference.MSTestTestFramework(Latest)).Verify();

#if NET

    [TestMethod]
    public void TestMethodShouldContainAssertion_CSharp9() =>
        builder.AddPaths("TestMethodShouldContainAssertion.CSharp9.cs")
            .AddReferences(NuGetMetadataReference.MSTestTestFrameworkV1)
            .AddReferences(NuGetMetadataReference.XunitFramework(Latest))
            .AddReferences(NuGetMetadataReference.NUnit(Latest))
            .WithOptions(ParseOptionsHelper.FromCSharp9)
            .Verify();

    [TestMethod]
    public void TestMethodShouldContainAssertion_CSharp11() =>
        builder.AddPaths("TestMethodShouldContainAssertion.CSharp11.cs")
            .AddReferences(NuGetMetadataReference.MSTestTestFrameworkV1)
            .WithOptions(ParseOptionsHelper.FromCSharp11)
            .Verify();

#endif

    internal static VerifierBuilder WithTestReferences(IEnumerable<MetadataReference> testFrameworkReference,
                                                                          string fluentVersion = Latest,
                                                                          string nSubstituteVersion = Latest,
                                                                          string nFluentVersion = Latest,
                                                                          string shouldlyVersion = Latest,
                                                                          string moqVersion = Latest) =>
        new VerifierBuilder<TestMethodShouldContainAssertion>()
            .AddReferences(testFrameworkReference)
            .AddReferences(NuGetMetadataReference.FluentAssertions(fluentVersion))
            .AddReferences(NuGetMetadataReference.NSubstitute(nSubstituteVersion))
            .AddReferences(NuGetMetadataReference.NFluent(nFluentVersion))
            .AddReferences(NuGetMetadataReference.Shouldly(shouldlyVersion))
            .AddReferences(NuGetMetadataReference.Moq(moqVersion))
            .AddReferences(MetadataReferenceFacade.SystemData)
            .AddReferences(MetadataReferenceFacade.SystemNetHttp)
            .AddReferences(MetadataReferenceFacade.SystemXml)
            .AddReferences(MetadataReferenceFacade.SystemXmlLinq)
            .AddReferences(MetadataReferenceFacade.SystemThreadingTasks);

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
        public const string Ver3Latest = "3.14.0";
        public const string Ver25 = "2.5.7.10213";
        public const string Ver27 = "2.7.0";
    }
}
