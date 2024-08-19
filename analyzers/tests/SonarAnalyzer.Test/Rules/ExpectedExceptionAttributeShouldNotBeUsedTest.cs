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

using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class ExpectedExceptionAttributeShouldNotBeUsedTest
{
    private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.ExpectedExceptionAttributeShouldNotBeUsed>();
    private readonly VerifierBuilder builderVB = new VerifierBuilder<VB.ExpectedExceptionAttributeShouldNotBeUsed>();

    [DataTestMethod]
    [DataRow("1.1.11")]
    [DataRow(Constants.NuGetLatestVersion)]
    public void ExpectedExceptionAttributeShouldNotBeUsed_MsTest_CS(string testFwkVersion) =>
        builderCS.AddPaths("ExpectedExceptionAttributeShouldNotBeUsed.MsTest.cs")
            .AddReferences(NuGetMetadataReference.MSTestTestFramework(testFwkVersion))
            .Verify();

    [DataTestMethod]
    [DataRow("2.5.7.10213")] // Lowest NUnit version available
    [DataRow("2.7.1")] // Latest version of NUnit that contains the attribute
    public void ExpectedExceptionAttributeShouldNotBeUsed_NUnit_CS(string testFwkVersion) =>
        builderCS.AddPaths("ExpectedExceptionAttributeShouldNotBeUsed.NUnit.cs")
            .AddReferences(NuGetMetadataReference.NUnit(testFwkVersion))
            .Verify();

    [DataTestMethod]
    [DataRow("3.0.0")]
    [DataRow(Constants.NuGetLatestVersion)]
    [Description("Starting with version 3.0.0 the attribute was removed.")]
    public void ExpectedExceptionAttributeShouldNotBeUsed_NUnit_NoIssue_CS(string testFwkVersion) =>
        builderCS.AddPaths("ExpectedExceptionAttributeShouldNotBeUsed.NUnit.cs")
            .AddReferences(NuGetMetadataReference.NUnit(testFwkVersion))
            .WithErrorBehavior(CompilationErrorBehavior.Ignore)
            .VerifyNoIssues();

    [DataTestMethod]
    [DataRow("1.1.11")]
    [DataRow(Constants.NuGetLatestVersion)]
    public void ExpectedExceptionAttributeShouldNotBeUsed_MsTest_VB(string testFwkVersion) =>
        builderVB.AddPaths("ExpectedExceptionAttributeShouldNotBeUsed.MsTest.vb")
            .AddReferences(NuGetMetadataReference.MSTestTestFramework(testFwkVersion))
            .Verify();

    [DataTestMethod]
    [DataRow("2.5.7.10213")]
    [DataRow("2.6.7")]
    public void ExpectedExceptionAttributeShouldNotBeUsed_NUnit_VB(string testFwkVersion) =>
        builderVB.AddPaths("ExpectedExceptionAttributeShouldNotBeUsed.NUnit.vb")
            .AddReferences(NuGetMetadataReference.NUnit(testFwkVersion))
            .Verify();

    [DataTestMethod]
    [DataRow("3.0.0")]
    [DataRow(Constants.NuGetLatestVersion)]
    [Description("Starting with version 3.0.0 the attribute was removed.")]
    public void ExpectedExceptionAttributeShouldNotBeUsed_NUnit_NoIssue_VB(string testFwkVersion) =>
        builderVB.AddPaths("ExpectedExceptionAttributeShouldNotBeUsed.NUnit.vb")
            .AddReferences(NuGetMetadataReference.NUnit(testFwkVersion))
            .WithErrorBehavior(CompilationErrorBehavior.Ignore)
            .VerifyNoIssues();
}
