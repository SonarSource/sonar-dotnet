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

using System.Collections.Immutable;
using System.Linq;
using csharp::SonarAnalyzer.Rules.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class UriShouldNotBeHardcodedTest
    {
        [TestMethod]
        [TestCategory("Rule")]
        public void UriShouldNotBeHardcoded_CSharp_General()
        {
            Verifier.VerifyAnalyzer(@"TestCases\UriShouldNotBeHardcoded.cs",
                new UriShouldNotBeHardcoded());
        }

        [DataTestMethod]
        [TestCategory("Rule")]
        [DataRow("3.0.20105.1")]
        [DataRow(Constants.NuGetLatestVersion)]
        public void UriShouldNotBeHardcoded_CSharp_VirtualPath_AspNet(string aspNetMvcVersion)
        {
            Verifier.VerifyAnalyzer(@"TestCases\UriShouldNotBeHardcoded.AspNet.cs",
                new UriShouldNotBeHardcoded(),
                additionalReferences: FrameworkMetadataReference.SystemWeb
                    .Concat(NuGetMetadataReference.MicrosoftAspNetMvc(aspNetMvcVersion))
                    .ToImmutableArray());
        }

        [DataTestMethod]
        [TestCategory("Rule")]
        [DataRow("2.0.4", "2.0.3", "2.1.1")]
        [DataRow(Constants.NuGetLatestVersion, Constants.NuGetLatestVersion, Constants.NuGetLatestVersion)]
        public void UriShouldNotBeHardcoded_CSharp_VirtualPath_AspNetCore(string aspNetCoreMvcVersion, string aspNetCoreRoutingVersion, string netHttpHeadersVersion)
        {
            Verifier.VerifyAnalyzer(@"TestCases\UriShouldNotBeHardcoded.AspNetCore.cs",
                new UriShouldNotBeHardcoded(),
                additionalReferences:
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
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void UriShouldNotBeHardcoded_VB()
        {
            Verifier.VerifyAnalyzer(@"TestCases\UriShouldNotBeHardcoded.vb",
                new SonarAnalyzer.Rules.VisualBasic.UriShouldNotBeHardcoded());
        }
    }
}
