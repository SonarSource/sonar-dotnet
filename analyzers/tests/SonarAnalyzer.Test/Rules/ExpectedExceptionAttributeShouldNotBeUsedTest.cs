/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using static SonarAnalyzer.TestFramework.MetadataReferences.NugetPackageVersions;
using CS = SonarAnalyzer.CSharp.Rules;
using VB = SonarAnalyzer.VisualBasic.Rules;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class ExpectedExceptionAttributeShouldNotBeUsedTest
{
    private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.ExpectedExceptionAttributeShouldNotBeUsed>();
    private readonly VerifierBuilder builderVB = new VerifierBuilder<VB.ExpectedExceptionAttributeShouldNotBeUsed>();

    [TestMethod]
    [DataRow(MsTest.Ver1_1)]
    [DataRow(MsTest.Ver3)]
    // Removed in V4 https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-mstest-migration-v3-v4#expectedexceptionattribute-api-is-removed
    public void ExpectedExceptionAttributeShouldNotBeUsed_MsTest_CS(string testFwkVersion) =>
        builderCS.AddPaths("ExpectedExceptionAttributeShouldNotBeUsed.MsTest.cs")
            .AddReferences(NuGetMetadataReference.MSTestTestFramework(testFwkVersion))
            .Verify();

    [TestMethod]
    [DataRow(NUnit.Ver25)] // Lowest NUnit version available
    [DataRow(NUnit.Ver27)] // Latest version of NUnit that contains the attribute
    public void ExpectedExceptionAttributeShouldNotBeUsed_NUnit_CS(string testFwkVersion) =>
        builderCS.AddPaths("ExpectedExceptionAttributeShouldNotBeUsed.NUnit.cs")
            .AddReferences(NuGetMetadataReference.NUnit(testFwkVersion))
            .Verify();

    [TestMethod]
    [DataRow("3.0.0")]
    [DataRow(TestConstants.NuGetLatestVersion)]
    [Description("Starting with version 3.0.0 the attribute was removed.")]
    public void ExpectedExceptionAttributeShouldNotBeUsed_NUnit_NoIssue_CS(string testFwkVersion) =>
        builderCS.AddPaths("ExpectedExceptionAttributeShouldNotBeUsed.NUnit.cs")
            .AddReferences(NuGetMetadataReference.NUnit(testFwkVersion))
            .WithErrorBehavior(CompilationErrorBehavior.Ignore)
            .VerifyNoIssues();

    [TestMethod]
    [DataRow(MsTest.Ver1_1)]
    [DataRow(MsTest.Ver3)]
    public void ExpectedExceptionAttributeShouldNotBeUsed_MsTest_VB(string testFwkVersion) =>
        builderVB.AddPaths("ExpectedExceptionAttributeShouldNotBeUsed.MsTest.vb")
            .AddReferences(NuGetMetadataReference.MSTestTestFramework(testFwkVersion))
            .Verify();

    [TestMethod]
    [DataRow("2.5.7.10213")]
    [DataRow("2.6.7")]
    public void ExpectedExceptionAttributeShouldNotBeUsed_NUnit_VB(string testFwkVersion) =>
        builderVB.AddPaths("ExpectedExceptionAttributeShouldNotBeUsed.NUnit.vb")
            .AddReferences(NuGetMetadataReference.NUnit(testFwkVersion))
            .Verify();

    [TestMethod]
    [DataRow("3.0.0")]
    [DataRow(TestConstants.NuGetLatestVersion)]
    [Description("Starting with version 3.0.0 the attribute was removed.")]
    public void ExpectedExceptionAttributeShouldNotBeUsed_NUnit_NoIssue_VB(string testFwkVersion) =>
        builderVB.AddPaths("ExpectedExceptionAttributeShouldNotBeUsed.NUnit.vb")
            .AddReferences(NuGetMetadataReference.NUnit(testFwkVersion))
            .WithErrorBehavior(CompilationErrorBehavior.Ignore)
            .VerifyNoIssues();
}
