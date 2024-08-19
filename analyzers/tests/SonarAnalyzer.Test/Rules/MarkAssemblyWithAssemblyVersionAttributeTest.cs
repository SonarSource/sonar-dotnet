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
public class MarkAssemblyWithAssemblyVersionAttributeTest
{
    private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.MarkAssemblyWithAssemblyVersionAttribute>();
    private readonly VerifierBuilder builderVB = new VerifierBuilder<VB.MarkAssemblyWithAssemblyVersionAttribute>();

    [TestMethod]
    public void MarkAssemblyWithAssemblyVersionAttribute_CS() =>
        builderCS.AddPaths("MarkAssemblyWithAssemblyVersionAttribute.cs").WithConcurrentAnalysis(false).VerifyNoIssues();

    [TestMethod]
    public void MarkAssemblyWithAssemblyVersionAttributeRazor_CS() =>
        builderCS
            .AddPaths("MarkAssemblyWithAssemblyVersionAttributeRazor.cs")
            .WithConcurrentAnalysis(false)
            .AddReferences(GetAspNetCoreRazorReferences())
            .VerifyNoIssues();

    [TestMethod]
    public void MarkAssemblyWithAssemblyVersionAttribute_CS_Concurrent() =>
        builderCS
            .AddPaths("MarkAssemblyWithAssemblyVersionAttribute.cs", "MarkAssemblyWithAssemblyVersionAttributeRazor.cs")
            .AddReferences(GetAspNetCoreRazorReferences())
            .WithAutogenerateConcurrentFiles(false)
            .VerifyNoIssues();

    [TestMethod]
    public void MarkAssemblyWithAssemblyVersionAttributeNoncompliant_CS() =>
        builderCS.AddPaths("MarkAssemblyWithAssemblyVersionAttributeNoncompliant.cs")
            .WithConcurrentAnalysis(false)
            .Verify();

    [TestMethod]
    public void MarkAssemblyWithAssemblyVersionAttributeNoncompliant_NoTargets_ShouldNotRaise_CS() =>
        // False positive. No assembly gets generated when Microsoft.Build.NoTargets is referenced.
        builderCS.AddSnippet("// Noncompliant ^1#0 {{Provide an 'AssemblyVersion' attribute for assembly 'project0'.}}")
            .AddReferences(NuGetMetadataReference.MicrosoftBuildNoTargets())
            .WithConcurrentAnalysis(false)
            .Verify();

    [TestMethod]
    public void MarkAssemblyWithAssemblyVersionAttribute_VB() =>
        builderVB.AddPaths("MarkAssemblyWithAssemblyVersionAttribute.vb").WithConcurrentAnalysis(false).VerifyNoIssues();

    [TestMethod]
    public void MarkAssemblyWithAssemblyVersionAttributeRazor_VB() =>
        builderVB
            .AddPaths("MarkAssemblyWithAssemblyVersionAttributeRazor.vb")
            .WithConcurrentAnalysis(false)
            .AddReferences(GetAspNetCoreRazorReferences())
            .VerifyNoIssues();

    [TestMethod]
    public void MarkAssemblyWithAssemblyVersionAttribute_VB_Concurrent() =>
        builderVB
            .AddPaths("MarkAssemblyWithAssemblyVersionAttribute.vb", "MarkAssemblyWithAssemblyVersionAttributeRazor.vb")
            .AddReferences(GetAspNetCoreRazorReferences())
            .WithAutogenerateConcurrentFiles(false)
            .VerifyNoIssues();

    [TestMethod]
    public void MarkAssemblyWithAssemblyVersionAttributeNoncompliant_VB() =>
        builderVB.AddPaths("MarkAssemblyWithAssemblyVersionAttributeNoncompliant.vb")
            .WithConcurrentAnalysis(false)
            .Verify();

    [TestMethod]
    public void MarkAssemblyWithAssemblyVersionAttributeNoncompliant_NoTargets_ShouldNotRaise_VB() =>
        // False positive. No assembly gets generated when Microsoft.Build.NoTargets is referenced.
        builderVB.AddSnippet("' Noncompliant ^1#0 {{Provide an 'AssemblyVersion' attribute for assembly 'project0'.}}")
            .AddReferences(NuGetMetadataReference.MicrosoftBuildNoTargets())
            .WithConcurrentAnalysis(false)
            .Verify();

    private static IEnumerable<MetadataReference> GetAspNetCoreRazorReferences() =>
        NuGetMetadataReference.MicrosoftAspNetCoreMvcRazorRuntime(Constants.NuGetLatestVersion);
}
