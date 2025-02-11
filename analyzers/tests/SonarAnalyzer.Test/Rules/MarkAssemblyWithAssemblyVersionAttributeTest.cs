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

using CS = SonarAnalyzer.CSharp.Rules;
using VB = SonarAnalyzer.VisualBasic.Rules;

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
        NuGetMetadataReference.MicrosoftAspNetCoreMvcRazorRuntime(TestConstants.NuGetLatestVersion);
}
