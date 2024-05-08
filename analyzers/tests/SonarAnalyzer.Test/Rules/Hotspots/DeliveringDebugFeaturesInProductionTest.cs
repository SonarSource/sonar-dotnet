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
            .VerifyNoIssues();  // No issues in test code

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
                  .Concat(NuGetMetadataReference.MicrosoftAspNetCoreDiagnostics(Constants.DotNetCore220Version))
                  .Concat(NuGetMetadataReference.MicrosoftAspNetCoreDiagnosticsEntityFrameworkCore(Constants.DotNetCore220Version))
                  .Concat(NuGetMetadataReference.MicrosoftAspNetCoreHttpAbstractions(Constants.DotNetCore220Version))
                  .Concat(NuGetMetadataReference.MicrosoftAspNetCoreHostingAbstractions(Constants.DotNetCore220Version));
}
