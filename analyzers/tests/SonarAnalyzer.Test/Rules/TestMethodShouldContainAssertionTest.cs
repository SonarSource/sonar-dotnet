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

using SonarAnalyzer.CSharp.Rules;
using SonarAnalyzer.Test.Common;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class TestMethodShouldContainAssertionTest
{
    private const string Latest = TestConstants.NuGetLatestVersion; // Rename only

    private readonly VerifierBuilder builder = new VerifierBuilder<TestMethodShouldContainAssertion>();

    [TestMethod]
    [DataRow(MsTestVersions.Ver1)]
    [DataRow(Latest)]
    public void TestMethodShouldContainAssertion_MSTest(string testFwkVersion) =>
        WithTestReferences(NuGetMetadataReference.MSTestTestFramework(testFwkVersion))
            .AddPaths("TestMethodShouldContainAssertion.MsTest.cs", "TestMethodShouldContainAssertion.MsTest.AnotherFile.cs")
            .Verify();

    [TestMethod]
    [DataRow(NUnitVersions.Ver3, Latest, Latest)]
    [DataRow(NUnitVersions.Ver3Latest, FluentAssertionVersions.Ver5, Latest)] // Breaking changes in NUnit 4.0 would fail the test https://github.com/SonarSource/sonar-dotnet/issues/8409
    [DataRow(NUnitVersions.Ver3Latest, Latest, Latest)] // Breaking changes in NUnit 4.0 would fail the test https://github.com/SonarSource/sonar-dotnet/issues/8409
    public void TestMethodShouldContainAssertion_NUnit(string testFwkVersion, string fluentVersion, string nSubstituteVersion) =>
        WithTestReferences(NuGetMetadataReference.NUnit(testFwkVersion), fluentVersion, nSubstituteVersion).AddPaths("TestMethodShouldContainAssertion.NUnit.cs").Verify();

    [TestMethod]
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

    [TestMethod]
    [DataRow(XUnitVersions.Ver2, Latest, Latest)]
    [DataRow(XUnitVersions.Ver253, Latest, Latest)]
    public void TestMethodShouldContainAssertion_Xunit(string testFwkVersion, string fluentVersion, string nSubstituteVersion) =>
        WithTestReferences(NuGetMetadataReference.XunitFramework(testFwkVersion), fluentVersion, nSubstituteVersion).AddPaths("TestMethodShouldContainAssertion.Xunit.cs").Verify();

    [TestMethod]
    public void TestMethodShouldContainAssertion_Xunit_Legacy() =>
        WithTestReferences(NuGetMetadataReference.XunitFrameworkV1).AddPaths("TestMethodShouldContainAssertion.Xunit.Legacy.cs").Verify();

    [TestMethod]
    public void TestMethodShouldContainAssertion_XunitV3() =>
        WithTestReferences(NuGetMetadataReference.XunitFrameworkV3(TestConstants.NuGetLatestVersion))
            .AddPaths("TestMethodShouldContainAssertion.Xunit.cs")
            .AddPaths("TestMethodShouldContainAssertion.XunitV3.cs")
            .AddReferences(NuGetMetadataReference.SystemMemory(TestConstants.NuGetLatestVersion))
            .AddReferences(MetadataReferenceFacade.NetStandard)
            .AddReferences(MetadataReferenceFacade.SystemCollections)
            .Verify();

    [TestMethod]
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

    [TestMethod]
    public void TestMethodShouldContainAssertion_FsCheck_XUnit() =>
        WithTestReferences(NuGetMetadataReference.FsCheckXunit(Latest))
            .AddPaths("TestMethodShouldContainAssertion.XUnit.FsCheck.cs")
            .VerifyNoIssues();

    [TestMethod]
    public void TestMethodShouldContainAssertion_FsCheck_NUnit() =>
        WithTestReferences(NuGetMetadataReference.FsCheckNunit(Latest))
            .AddPaths("TestMethodShouldContainAssertion.NUnit.FsCheck.cs")
            .VerifyNoIssues();

    [TestMethod]
    public void TestMethodShouldContainAssertion_CodeGenerator() =>
        builder
            .AddPaths("TestMethodShouldContainAssertion.SourceGenerators.cs")
            .AddReferences(NuGetMetadataReference.MSTestTestFramework(TestConstants.NuGetLatestVersion))
            .AddReferences(NuGetMetadataReference.MicrosoftCodeAnalysisCSharp())
            .AddReferences(NuGetMetadataReference.MicrosoftCodeAnalysisCSharpSourceGeneratorsTesting())
            .AddReferences(NuGetMetadataReference.MicrosoftCodeAnalysisAnalyzerTesting())
            .WithOptions(LanguageOptions.FromCSharp13)
            .Verify();

#if NET

    [TestMethod]
    public void TestMethodShouldContainAssertion_CSharp9() =>
        builder.AddPaths("TestMethodShouldContainAssertion.CSharp9.cs")
            .AddReferences(NuGetMetadataReference.MSTestTestFrameworkV1)
            .AddReferences(NuGetMetadataReference.XunitFramework(Latest))
            .AddReferences(NuGetMetadataReference.NUnit(Latest))
            .WithOptions(LanguageOptions.FromCSharp9)
            .Verify();

    [TestMethod]
    public void TestMethodShouldContainAssertion_CSharp11() =>
        builder.AddPaths("TestMethodShouldContainAssertion.CSharp11.cs")
            .AddReferences(NuGetMetadataReference.MSTestTestFrameworkV1)
            .WithOptions(LanguageOptions.FromCSharp11)
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
