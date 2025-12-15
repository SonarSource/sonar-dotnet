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

using SonarAnalyzer.CSharp.Rules;
using SonarAnalyzer.Test.Common;

using static SonarAnalyzer.TestFramework.MetadataReferences.NugetPackageVersions;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class TestMethodShouldContainAssertionTest
{
    private readonly VerifierBuilder builder = new VerifierBuilder<TestMethodShouldContainAssertion>();

    [TestMethod]
    [DataRow(MsTest.Ver1_1)]
    [DataRow(MsTest.Ver3)]
    [DataRow(Latest)]
    public void TestMethodShouldContainAssertion_MSTest_Common(string testFwkVersion) =>
        WithTestReferences(NuGetMetadataReference.MSTestTestFramework(testFwkVersion))
            .AddPaths("TestMethodShouldContainAssertion.MsTest.Common.cs", "TestMethodShouldContainAssertion.MsTest.AnotherFile.cs")
            .Verify();

    [TestMethod]
    public void TestMethodShouldContainAssertion_MSTest_V3() =>
        WithTestReferences(NuGetMetadataReference.MSTestTestFrameworkV3)
            .AddPaths("TestMethodShouldContainAssertion.MsTest.V3.cs", "TestMethodShouldContainAssertion.MsTest.AnotherFile.cs")
            .VerifyNoIssues();

    [TestMethod]
    public void TestMethodShouldContainAssertion_MSTest_Latest() =>
        WithTestReferences(NuGetMetadataReference.MSTestTestFramework(TestConstants.NuGetLatestVersion))
            .AddPaths("TestMethodShouldContainAssertion.MsTest.Latest.cs", "TestMethodShouldContainAssertion.MsTest.AnotherFile.cs")
            .Verify();

    [TestMethod]
    [DataRow(NUnit.Ver3, Latest, Latest)]
    [DataRow(NUnit.Ver3Latest, FluentAssertionsVersions.Ver5, Latest)] // Breaking changes in NUnit 4.0 would fail the test https://github.com/SonarSource/sonar-dotnet/issues/8409
    [DataRow(NUnit.Ver3Latest, Latest, Latest)] // Breaking changes in NUnit 4.0 would fail the test https://github.com/SonarSource/sonar-dotnet/issues/8409
    public void TestMethodShouldContainAssertion_NUnit(string testFwkVersion, string fluentVersion, string nSubstituteVersion) =>
        WithTestReferences(NuGetMetadataReference.NUnit(testFwkVersion), fluentVersion, nSubstituteVersion).AddPaths("TestMethodShouldContainAssertion.NUnit.cs").Verify();

    [TestMethod]
    [DataRow(NUnit.Ver25)]
    [DataRow(NUnit.Ver27)]
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
    [DataRow(NUnit.Ver25, FluentAssertionsVersions.Ver1)]
    [DataRow(NUnit.Ver25, FluentAssertionsVersions.Ver4)]
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
       WithTestReferences(NuGetMetadataReference.NUnit(NUnit.Ver25), nFluentVersion: "1.3.1").AddSnippet("""
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
    [DataRow(MsTest.Ver1_1)]
    [DataRow(MsTest.Ver3)]
    [DataRow(Latest)]
    public void TestMethodShouldContainAssertion_CustomAssertionMethod_Common(string version) =>
        builder.AddPaths("TestMethodShouldContainAssertion.Custom.Common.cs").AddReferences(NuGetMetadataReference.MSTestTestFramework(version)).Verify();

    [TestMethod]
    public void TestMethodShouldContainAssertion_CustomAssertionMethod_V3() =>
        builder.AddPaths("TestMethodShouldContainAssertion.Custom.V3.cs").AddReferences(NuGetMetadataReference.MSTestTestFrameworkV3).VerifyNoIssues();

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

    [TestMethod]
    public void TestMethodShouldContainAssertion_Latest() =>
        builder.AddPaths("TestMethodShouldContainAssertion.Latest.cs")
            .AddReferences(NuGetMetadataReference.MSTestTestFrameworkV1)
            .AddReferences(NuGetMetadataReference.XunitFramework(Latest))
            .AddReferences(NuGetMetadataReference.NUnit(Latest))
            .WithOptions(LanguageOptions.CSharpLatest)
            .Verify();

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
}
