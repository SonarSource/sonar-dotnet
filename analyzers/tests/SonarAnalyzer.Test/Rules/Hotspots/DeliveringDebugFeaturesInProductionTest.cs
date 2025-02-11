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
public class DeliveringDebugFeaturesInProductionTest
{
    private readonly VerifierBuilder builderCS = new VerifierBuilder().WithBasePath("Hotspots").AddAnalyzer(() => new CS.DeliveringDebugFeaturesInProduction(AnalyzerConfiguration.AlwaysEnabled));
    private readonly VerifierBuilder builderVB = new VerifierBuilder().WithBasePath("Hotspots").AddAnalyzer(() => new VB.DeliveringDebugFeaturesInProduction(AnalyzerConfiguration.AlwaysEnabled));

    [TestMethod]
    public void DeliveringDebugFeaturesInProduction_NetCore2_CS() =>
        builderCS.AddPaths("DeliveringDebugFeaturesInProduction.NetCore2.cs")
            .AddReferences(AdditionalReferencesForAspNetCore2)
            .Verify();

    [TestMethod]
    public void DeliveringDebugFeaturesInProduction_NetCore2_VB() =>
        builderVB.AddPaths("DeliveringDebugFeaturesInProduction.NetCore2.vb")
            .AddReferences(AdditionalReferencesForAspNetCore2)
            .Verify();

#if NET

    [TestMethod]
    public void DeliveringDebugFeaturesInProduction_NetCore3_CS() =>
        builderCS.AddPaths("DeliveringDebugFeaturesInProduction.NetCore3.cs")
            .AddReferences(AdditionalReferencesForAspNetCore3AndLater)
            .Verify();

    [TestMethod]
    public void DeliveringDebugFeaturesInProduction_NetCore3_VB() =>
        builderVB.AddPaths("DeliveringDebugFeaturesInProduction.NetCore3.vb")
            .AddReferences(AdditionalReferencesForAspNetCore3AndLater)
            .Verify();

    [TestMethod]
    public void DeliveringDebugFeaturesInProduction_Net7_CS() =>
        builderCS.AddPaths("DeliveringDebugFeaturesInProduction.Net7.cs")
            .WithTopLevelStatements()
            .AddReferences([
                AspNetCoreMetadataReference.MicrosoftAspNetCore,
                AspNetCoreMetadataReference.MicrosoftAspNetCoreRouting,
                AspNetCoreMetadataReference.MicrosoftAspNetCoreDiagnostics,
                AspNetCoreMetadataReference.MicrosoftAspNetCoreHttpAbstractions,
                AspNetCoreMetadataReference.MicrosoftAspNetCoreHostingAbstractions,
                AspNetCoreMetadataReference.MicrosoftExtensionsHostingAbstractions])
            .VerifyNoIssues();

    private static IEnumerable<MetadataReference> AdditionalReferencesForAspNetCore3AndLater =>
        new[]
        {
            AspNetCoreMetadataReference.MicrosoftAspNetCoreDiagnostics,
            AspNetCoreMetadataReference.MicrosoftAspNetCoreHostingAbstractions,
            AspNetCoreMetadataReference.MicrosoftAspNetCoreHttpAbstractions,
            AspNetCoreMetadataReference.MicrosoftExtensionsHostingAbstractions
        };

#endif

    internal static IEnumerable<MetadataReference> AdditionalReferencesForAspNetCore2 =>
        Enumerable.Empty<MetadataReference>()
                  .Concat(MetadataReferenceFacade.NetStandard)
                  .Concat(NuGetMetadataReference.MicrosoftAspNetCoreDiagnostics(TestConstants.DotNetCore220Version))
                  .Concat(NuGetMetadataReference.MicrosoftAspNetCoreDiagnosticsEntityFrameworkCore(TestConstants.DotNetCore220Version))
                  .Concat(NuGetMetadataReference.MicrosoftAspNetCoreHttpAbstractions(TestConstants.DotNetCore220Version))
                  .Concat(NuGetMetadataReference.MicrosoftAspNetCoreHostingAbstractions(TestConstants.DotNetCore220Version));
}
