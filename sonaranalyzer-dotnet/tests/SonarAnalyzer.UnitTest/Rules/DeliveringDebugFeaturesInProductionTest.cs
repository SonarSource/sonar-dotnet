﻿/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2020 SonarSource SA
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
using SonarAnalyzer.UnitTest.MetadataReferences;
using SonarAnalyzer.UnitTest.TestFramework;
using CSharp = csharp::SonarAnalyzer.Rules.CSharp;
using VisualBasic = vbnet::SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class DeliveringDebugFeaturesInProductionTest
    {
        [TestMethod]
        [TestCategory("Rule")]
        public void DeliveringDebugFeaturesInProduction_NetCore2_CS() =>
            Verifier.VerifyAnalyzer(@"TestCases\DeliveringDebugFeaturesInProduction.NetCore2.cs",
                new CSharp.DeliveringDebugFeaturesInProduction(AnalyzerConfiguration.AlwaysEnabled),
                additionalReferences: AdditionalReferencesNetCore2);

        [TestMethod]
        [TestCategory("Rule")]
        public void DeliveringDebugFeaturesInProduction_NetCore2_CS_Disabled() =>
            Verifier.VerifyNoIssueReported(@"TestCases\DeliveringDebugFeaturesInProduction.NetCore2.cs",
                new CSharp.DeliveringDebugFeaturesInProduction(),
                additionalReferences: AdditionalReferencesNetCore2);

        [TestMethod]
        [TestCategory("Rule")]
        public void DeliveringDebugFeaturesInProduction_NetCore3_CS() =>
            Verifier.VerifyAnalyzer(@"TestCases\DeliveringDebugFeaturesInProduction.NetCore3.cs",
                new CSharp.DeliveringDebugFeaturesInProduction(AnalyzerConfiguration.AlwaysEnabled),
                additionalReferences: AdditionalReferencesNetCore3);

        [TestMethod]
        [TestCategory("Rule")]
        public void DeliveringDebugFeaturesInProduction_NetCore2_VB() =>
            Verifier.VerifyAnalyzer(@"TestCases\DeliveringDebugFeaturesInProduction.NetCore2.vb",
                new VisualBasic.DeliveringDebugFeaturesInProduction(AnalyzerConfiguration.AlwaysEnabled),
                additionalReferences: AdditionalReferencesNetCore2);

        [TestMethod]
        [TestCategory("Rule")]
        public void DeliveringDebugFeaturesInProduction_NetCore2_VB_Disabled() =>
            Verifier.VerifyNoIssueReported(@"TestCases\DeliveringDebugFeaturesInProduction.NetCore2.vb",
                new VisualBasic.DeliveringDebugFeaturesInProduction(),
                additionalReferences: AdditionalReferencesNetCore2);

        [TestMethod]
        [TestCategory("Rule")]
        public void DeliveringDebugFeaturesInProduction_NetCore3_VB() =>
            Verifier.VerifyAnalyzer(@"TestCases\DeliveringDebugFeaturesInProduction.NetCore3.vb",
                new VisualBasic.DeliveringDebugFeaturesInProduction(AnalyzerConfiguration.AlwaysEnabled),
                additionalReferences: AdditionalReferencesNetCore3);

        internal static IEnumerable<MetadataReference> AdditionalReferencesNetCore2 =>
            Enumerable.Empty<MetadataReference>()
                .Concat(NetStandardMetadataReference.Netstandard)
                .Concat(NuGetMetadataReference.MicrosoftAspNetCoreDiagnostics(Constants.DotNetCore220Version))
                .Concat(NuGetMetadataReference.MicrosoftAspNetCoreDiagnosticsEntityFrameworkCore(Constants.DotNetCore220Version))
                .Concat(NuGetMetadataReference.MicrosoftAspNetCoreHttpAbstractions(Constants.DotNetCore220Version))
                .Concat(NuGetMetadataReference.MicrosoftAspNetCoreHostingAbstractions(Constants.DotNetCore220Version));

        //FIXME: Rework this
        internal static IEnumerable<MetadataReference> AdditionalReferencesNetCore3 =>
            Enumerable.Empty<MetadataReference>()
                .Concat(new[] { MetadataReference.CreateFromFile(@"c:\Program Files (x86)\dotnet\shared\Microsoft.AspNetCore.App\3.1.4\Microsoft.AspNetCore.Http.Abstractions.dll") })
                .Concat(new[] { MetadataReference.CreateFromFile(@"c:\Program Files (x86)\dotnet\shared\Microsoft.AspNetCore.App\3.1.4\Microsoft.AspNetCore.Hosting.Abstractions.dll") })
                .Concat(new[] { MetadataReference.CreateFromFile(@"c:\Program Files (x86)\dotnet\shared\Microsoft.AspNetCore.App\3.1.4\Microsoft.Extensions.Hosting.Abstractions.dll") })
                .Concat(new[] { MetadataReference.CreateFromFile(@"c:\Program Files (x86)\dotnet\shared\Microsoft.AspNetCore.App\3.1.4\Microsoft.AspNetCore.Diagnostics.dll") });
    }
}
