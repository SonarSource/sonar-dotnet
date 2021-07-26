/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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

using System.Collections.Immutable;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.UnitTest.MetadataReferences;
using SonarAnalyzer.UnitTest.TestFramework;
using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class UriShouldNotBeHardcodedTest
    {
        [TestMethod]
        [TestCategory("Rule")]
        public void UriShouldNotBeHardcoded_CSharp_General() =>
            Verifier.VerifyConcurrentAnalyzer(@"TestCases\UriShouldNotBeHardcoded.cs",
                                    new CS.UriShouldNotBeHardcoded());

#if NETFRAMEWORK // HttpContext is available only when targeting .Net Framework
        [DataTestMethod]
        [TestCategory("Rule")]
        [DataRow("3.0.20105.1")]
        [DataRow(Constants.NuGetLatestVersion)]
        public void UriShouldNotBeHardcoded_CSharp_VirtualPath_AspNet(string aspNetMvcVersion) =>
            Verifier.VerifyAnalyzer(@"TestCases\UriShouldNotBeHardcoded.AspNet.cs",
                                    new CS.UriShouldNotBeHardcoded(),
                                    MetadataReferenceFacade.SystemWeb.Concat(NuGetMetadataReference.MicrosoftAspNetMvc(aspNetMvcVersion)));
#endif

        [DataTestMethod]
        [TestCategory("Rule")]
        [DataRow("2.0.4", "2.0.3", "2.1.1")]
        [DataRow(Constants.NuGetLatestVersion, Constants.NuGetLatestVersion, Constants.NuGetLatestVersion)]
        public void UriShouldNotBeHardcoded_CSharp_VirtualPath_AspNetCore(string aspNetCoreMvcVersion, string aspNetCoreRoutingVersion, string netHttpHeadersVersion) =>
            Verifier.VerifyAnalyzer(@"TestCases\UriShouldNotBeHardcoded.AspNetCore.cs",
                new CS.UriShouldNotBeHardcoded(),
                // for VirtualFileResult
                NuGetMetadataReference.MicrosoftAspNetCoreMvcCore(aspNetCoreMvcVersion)
                    // for Controller
                    .Concat(NuGetMetadataReference.MicrosoftAspNetCoreMvcViewFeatures(aspNetCoreMvcVersion))
                    // for IRouter and VirtualPathData
                    .Concat(NuGetMetadataReference.MicrosoftAspNetCoreRoutingAbstractions(aspNetCoreRoutingVersion))
                    // for IRouteBuilder
                    .Concat(NuGetMetadataReference.MicrosoftAspNetCoreRouting(aspNetCoreRoutingVersion))
                    // for IActionResult
                    .Concat(NuGetMetadataReference.MicrosoftAspNetCoreMvcAbstractions(aspNetCoreMvcVersion))
                    .Concat(NuGetMetadataReference.MicrosoftNetHttpHeaders(netHttpHeadersVersion))
                    .ToImmutableArray());

        [TestMethod]
        [TestCategory("Rule")]
        public void UriShouldNotBeHardcoded_VB() =>
            Verifier.VerifyConcurrentAnalyzer(@"TestCases\UriShouldNotBeHardcoded.vb",
                                    new VB.UriShouldNotBeHardcoded());
    }
}
