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

using SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class TestMethodShouldNotBeIgnoredTest
{
    private readonly VerifierBuilder builder = new VerifierBuilder<TestMethodShouldNotBeIgnored>().AddReferences(NuGetMetadataReference.MSTestTestFrameworkV1);

    [TestMethod]
    public void TestMethodShouldNotBeIgnored_MsTest_Legacy() =>
        builder.AddPaths("TestMethodShouldNotBeIgnored.MsTest.cs")
            .WithErrorBehavior(CompilationErrorBehavior.Ignore)    // IgnoreAttribute doesn't contain any reason param
            .Verify();

    [DataTestMethod]
    [DataRow("1.2.0")]
    [DataRow(TestConstants.NuGetLatestVersion)]
    public void TestMethodShouldNotBeIgnored_MsTest(string testFwkVersion) =>
        new VerifierBuilder<TestMethodShouldNotBeIgnored>()
            .AddPaths("TestMethodShouldNotBeIgnored.MsTest.cs")
            .AddReferences(NuGetMetadataReference.MSTestTestFramework(testFwkVersion)).Verify();

    [TestMethod]
    public void TestMethodShouldNotBeIgnored_MsTest_InvalidCases() =>
        builder.AddSnippet("""
            using Microsoft.VisualStudio.TestTools.UnitTesting;
            namespace Tests.Diagnostics.TestMethods
            {
                [ThisDoesNotExist]
                public class MsTestClass3
                {
                }

                [Ignore]
            }
            """)
            .VerifyNoIssuesIgnoreErrors();

    [DataTestMethod]
    [DataRow("2.5.7.10213")]
    [DataRow("2.7.0")]
    public void TestMethodShouldNotBeIgnored_NUnit_V2(string testFwkVersion) =>
        builder.AddPaths("TestMethodShouldNotBeIgnored.NUnit.V2.cs")
            .AddReferences(NuGetMetadataReference.NUnit(testFwkVersion))
            .Verify();

    [DataTestMethod]
    [DataRow("3.0.0")] // Ignore without reason no longer exist
    [DataRow(TestConstants.NuGetLatestVersion)]
    public void TestMethodShouldNotBeIgnored_NUnit(string testFwkVersion) =>
        builder.AddPaths("TestMethodShouldNotBeIgnored.NUnit.cs").AddReferences(NuGetMetadataReference.NUnit(testFwkVersion)).Verify();

    [DataTestMethod]
    [DataRow("2.0.0")]
    [DataRow(TestConstants.NuGetLatestVersion)]
    public void TestMethodShouldNotBeIgnored_Xunit(string testFwkVersion) =>
        builder.AddPaths("TestMethodShouldNotBeIgnored.Xunit.cs").AddReferences(NuGetMetadataReference.XunitFramework(testFwkVersion)).VerifyNoIssues();

    [TestMethod]
    public void TestMethodShouldNotBeIgnored_Xunit_v1() =>
        builder.AddPaths("TestMethodShouldNotBeIgnored.Xunit.v1.cs").AddReferences(NuGetMetadataReference.XunitFrameworkV1).VerifyNoIssues();

#if NET

    [TestMethod]
    public void TestMethodShouldNotBeIgnored_CSharp9() =>
        builder.AddPaths("TestMethodShouldNotBeIgnored.CSharp9.cs")
            .AddReferences(NuGetMetadataReference.XunitFrameworkV1)
            .AddReferences(NuGetMetadataReference.NUnit(TestConstants.NuGetLatestVersion))
            .WithOptions(LanguageOptions.FromCSharp9)
            .Verify();

    [TestMethod]
    public void TestMethodShouldNotBeIgnored_CSharp11() =>
        builder.AddPaths("TestMethodShouldNotBeIgnored.CSharp11.cs")
            .AddReferences(NuGetMetadataReference.MSTestTestFramework(TestConstants.NuGetLatestVersion))
            .WithOptions(LanguageOptions.FromCSharp11)
            .Verify();

#endif

}
