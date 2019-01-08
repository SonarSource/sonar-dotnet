/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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

extern alias csharp;
extern alias vbnet;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Common;
using CSharp = csharp::SonarAnalyzer.Rules.CSharp;
using VisualBasic = vbnet::SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class DeliveringDebugFeaturesInProductionTest
    {
        [TestMethod]
        [TestCategory("Rule")]
        public void DeliveringDebugFeaturesInProduction_CS()
        {
            Verifier.VerifyAnalyzer(@"TestCases\DeliveringDebugFeaturesInProduction.cs",
                new CSharp.DeliveringDebugFeaturesInProduction(AnalyzerConfiguration.AlwaysEnabled),
                additionalReferences: AdditionalReferences);
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void DeliveringDebugFeaturesInProduction_CS_Disabled()
        {
            Verifier.VerifyNoIssueReported(@"TestCases\DeliveringDebugFeaturesInProduction.cs",
                new CSharp.DeliveringDebugFeaturesInProduction(),
                additionalReferences: AdditionalReferences);
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void DeliveringDebugFeaturesInProduction_VB()
        {
            Verifier.VerifyAnalyzer(@"TestCases\DeliveringDebugFeaturesInProduction.vb",
                new VisualBasic.DeliveringDebugFeaturesInProduction(AnalyzerConfiguration.AlwaysEnabled),
                additionalReferences: AdditionalReferences);
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void DeliveringDebugFeaturesInProduction_VB_Disabled()
        {
            Verifier.VerifyNoIssueReported(@"TestCases\DeliveringDebugFeaturesInProduction.vb",
                new VisualBasic.DeliveringDebugFeaturesInProduction(),
                additionalReferences: AdditionalReferences);
        }

        private static IEnumerable<MetadataReference> AdditionalReferences =>
            Enumerable.Empty<MetadataReference>()
                .Concat(FrameworkMetadataReference.Netstandard)
                .Concat(NuGetMetadataReference.MicrosoftAspNetCoreDiagnostics(Constants.NuGetLatestVersion))
                .Concat(NuGetMetadataReference.MicrosoftAspNetCoreDiagnosticsEntityFrameworkCore(Constants.NuGetLatestVersion))
                .Concat(NuGetMetadataReference.MicrosoftAspNetCoreHttpAbstractions(Constants.NuGetLatestVersion))
                .Concat(NuGetMetadataReference.MicrosoftAspNetCoreHostingAbstractions(Constants.NuGetLatestVersion));
    }
}
