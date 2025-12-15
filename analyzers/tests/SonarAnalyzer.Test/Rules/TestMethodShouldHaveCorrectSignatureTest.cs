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
using static SonarAnalyzer.TestFramework.MetadataReferences.NugetPackageVersions;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class TestMethodShouldHaveCorrectSignatureTest
{
    private readonly VerifierBuilder builder = new VerifierBuilder<TestMethodShouldHaveCorrectSignature>();

    [TestMethod]
    [DataRow(MsTest.Ver1_1)]
    [DataRow(MsTest.Ver3)]
    [DataRow(Latest)]
    public void TestMethodShouldHaveCorrectSignature_MsTest(string testFwkVersion) =>
        builder.AddPaths("TestMethodShouldHaveCorrectSignature.MsTest.cs")
            .AddReferences(NuGetMetadataReference.MSTestTestFramework(testFwkVersion))
            .Verify();

    [TestMethod]
    [DataRow(NUnit.Ver25)]
    [DataRow(Latest)]
    public void TestMethodShouldHaveCorrectSignature_NUnit(string testFwkVersion) =>
        builder.AddPaths("TestMethodShouldHaveCorrectSignature.NUnit.cs")
            .AddReferences(NuGetMetadataReference.NUnit(testFwkVersion))
            .Verify();

    [TestMethod]
    [DataRow("2.0.0")]
    [DataRow(TestConstants.NuGetLatestVersion)]
    public void TestMethodShouldHaveCorrectSignature_Xunit(string testFwkVersion) =>
        builder.AddPaths("TestMethodShouldHaveCorrectSignature.Xunit.cs")
            .AddReferences(NuGetMetadataReference.XunitFramework(testFwkVersion))
            .Verify();

    [TestMethod]
    public void TestMethodShouldHaveCorrectSignature_Xunit_Legacy() =>
        builder.AddPaths("TestMethodShouldHaveCorrectSignature.Xunit.Legacy.cs")
            .AddReferences(NuGetMetadataReference.XunitFrameworkV1)
            .Verify();

    [TestMethod]
    public void TestMethodShouldHaveCorrectSignature_XunitV3() =>
        builder.AddPaths("TestMethodShouldHaveCorrectSignature.Xunit.cs")
            .AddReferences(NuGetMetadataReference.XunitFrameworkV3(TestConstants.NuGetLatestVersion))
            .AddReferences(NuGetMetadataReference.SystemMemory(TestConstants.NuGetLatestVersion))
            .AddReferences(MetadataReferenceFacade.NetStandard)
            .AddReferences(MetadataReferenceFacade.SystemCollections)
            .Verify();

    [TestMethod]
    public void TestMethodShouldHaveCorrectSignature_MSTest_Miscellaneous() =>
        // Additional test cases e.g. partial classes, and methods with multiple faults.
        // We have to specify a test framework for the tests, but it doesn't really matter which
        // one, so we're using MSTest and only testing a single version.
        builder.AddPaths("TestMethodShouldHaveCorrectSignature.Misc.cs")
            .AddReferences(NuGetMetadataReference.MSTestTestFrameworkV1)
            .Verify();

    [TestMethod]
    public void TestMethodShouldHaveCorrectSignature_Latest() =>
        builder.AddPaths("TestMethodShouldHaveCorrectSignature.Latest.cs")
            .AddReferences(NuGetMetadataReference.MSTestTestFrameworkV1)
            .AddReferences(NuGetMetadataReference.XunitFramework(TestConstants.NuGetLatestVersion))
            .AddReferences(NuGetMetadataReference.NUnit(TestConstants.NuGetLatestVersion))
            .WithOptions(LanguageOptions.CSharpLatest)
            .Verify();
}
