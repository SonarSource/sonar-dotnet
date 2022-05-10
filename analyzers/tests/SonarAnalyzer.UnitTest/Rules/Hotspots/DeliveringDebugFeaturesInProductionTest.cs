/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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

using System.Linq;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.Common;
using SonarAnalyzer.UnitTest.MetadataReferences;
using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class DeliveringDebugFeaturesInProductionTest
    {
        [TestMethod]
        public void DeliveringDebugFeaturesInProduction_NetCore2_CS() =>
            OldVerifier.VerifyAnalyzer(
                @"TestCases\Hotspots\DeliveringDebugFeaturesInProduction.NetCore2.cs",
                new CS.DeliveringDebugFeaturesInProduction(AnalyzerConfiguration.AlwaysEnabled),
                AdditionalReferencesForAspNetCore2);

        [TestMethod]
        public void DeliveringDebugFeaturesInProduction_NetCore2_VB() =>
            OldVerifier.VerifyAnalyzer(
                @"TestCases\Hotspots\DeliveringDebugFeaturesInProduction.NetCore2.vb",
                new VB.DeliveringDebugFeaturesInProduction(AnalyzerConfiguration.AlwaysEnabled),
                AdditionalReferencesForAspNetCore2);

#if NET

        [TestMethod]
        public void DeliveringDebugFeaturesInProduction_NetCore3_CS() =>
            OldVerifier.VerifyAnalyzer(
                @"TestCases\Hotspots\DeliveringDebugFeaturesInProduction.NetCore3.cs",
                new CS.DeliveringDebugFeaturesInProduction(AnalyzerConfiguration.AlwaysEnabled),
                AdditionalReferencesForAspNetCore3AndLater);

        [TestMethod]
        public void DeliveringDebugFeaturesInProduction_NetCore3_VB() =>
            OldVerifier.VerifyAnalyzer(
                @"TestCases\Hotspots\DeliveringDebugFeaturesInProduction.NetCore3.vb",
                new VB.DeliveringDebugFeaturesInProduction(AnalyzerConfiguration.AlwaysEnabled),
                AdditionalReferencesForAspNetCore3AndLater);

        private static IEnumerable<MetadataReference> AdditionalReferencesForAspNetCore3AndLater =>
            new []
            {
                AspNetCoreMetadataReference.MicrosoftAspNetCoreDiagnostics,
                AspNetCoreMetadataReference.MicrosoftAspNetCoreHostingAbstractions,
                AspNetCoreMetadataReference.MicrosoftAspNetCoreHttpAbstractions,
                CoreMetadataReference.MicrosoftExtensionsHostingAbstractions
            };

#endif

        internal static IEnumerable<MetadataReference> AdditionalReferencesForAspNetCore2 =>
            Enumerable.Empty<MetadataReference>()
                      .Concat(NetStandardMetadataReference.Netstandard)
                      .Concat(NuGetMetadataReference.MicrosoftAspNetCoreDiagnostics(Constants.DotNetCore220Version))
                      .Concat(NuGetMetadataReference.MicrosoftAspNetCoreDiagnosticsEntityFrameworkCore(Constants.DotNetCore220Version))
                      .Concat(NuGetMetadataReference.MicrosoftAspNetCoreHttpAbstractions(Constants.DotNetCore220Version))
                      .Concat(NuGetMetadataReference.MicrosoftAspNetCoreHostingAbstractions(Constants.DotNetCore220Version));
    }
}
