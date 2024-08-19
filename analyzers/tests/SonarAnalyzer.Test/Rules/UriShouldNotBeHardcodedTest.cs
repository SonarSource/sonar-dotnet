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

namespace SonarAnalyzer.Test.Rules
{
    [TestClass]
    public class UriShouldNotBeHardcodedTest
    {
        private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.UriShouldNotBeHardcoded>();
        private readonly VerifierBuilder builderVB = new VerifierBuilder<VB.UriShouldNotBeHardcoded>();

        [TestMethod]
        public void UriShouldNotBeHardcoded_CSharp_General() =>
            builderCS.AddPaths("UriShouldNotBeHardcoded.cs").Verify();

        [TestMethod]
        public void UriShouldNotBeHardcoded_CSharp_Exceptions() =>
            builderCS
            .AddPaths("UriShouldNotBeHardcoded.Exceptions.cs")
            .AddReferences(MetadataReferenceFacade.SystemXml)
            .Verify();

#if NET

        [TestMethod]
        public void UriShouldNotBeHardcoded_CSharp11() =>
            builderCS.AddPaths("UriShouldNotBeHardcoded.CSharp11.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp11)
                .Verify();

        [TestMethod]
        public void UriShouldNotBeHardcoded_CSharp12() =>
            builderCS.AddPaths("UriShouldNotBeHardcoded.CSharp12.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp12)
                .Verify();

#endif

#if NETFRAMEWORK // HttpContext is available only when targeting .Net Framework

        [DataTestMethod]
        [DataRow("3.0.20105.1")]
        [DataRow(Constants.NuGetLatestVersion)]
        public void UriShouldNotBeHardcoded_CSharp_VirtualPath_AspNet(string aspNetMvcVersion) =>
            builderCS
                .AddPaths("UriShouldNotBeHardcoded.AspNet.cs")
                .AddReferences(MetadataReferenceFacade.SystemWeb.Concat(NuGetMetadataReference.MicrosoftAspNetMvc(aspNetMvcVersion)))
                .Verify();

#endif

        [DataTestMethod]
        [DataRow("2.0.4", "2.0.3", "2.1.1")]
        [DataRow("2.2.0", "2.2.0", "2.2.0")]
        public void UriShouldNotBeHardcoded_CSharp_VirtualPath_AspNetCore(string aspNetCoreMvcVersion, string aspNetCoreRoutingVersion, string netHttpHeadersVersion) =>
            builderCS
                .AddPaths("UriShouldNotBeHardcoded.AspNetCore.cs")
                .AddReferences(AdditionalReferences(aspNetCoreMvcVersion, aspNetCoreRoutingVersion, netHttpHeadersVersion))
                .Verify();

        [TestMethod]
        public void UriShouldNotBeHardcoded_VB() =>
            builderVB.AddPaths("UriShouldNotBeHardcoded.vb").Verify();

        private static IEnumerable<MetadataReference> AdditionalReferences(string aspNetCoreMvcVersion, string aspNetCoreRoutingVersion, string netHttpHeadersVersion) =>
            NuGetMetadataReference.MicrosoftAspNetCoreMvcCore(aspNetCoreMvcVersion)
                    // for Controller
                    .Concat(NuGetMetadataReference.MicrosoftAspNetCoreMvcViewFeatures(aspNetCoreMvcVersion))
                    // for IActionResult
                    .Concat(NuGetMetadataReference.MicrosoftAspNetCoreMvcAbstractions(aspNetCoreMvcVersion))
                    // for IRouter and VirtualPathData
                    .Concat(NuGetMetadataReference.MicrosoftAspNetCoreRoutingAbstractions(aspNetCoreRoutingVersion))
                    // for IRouteBuilder
                    .Concat(NuGetMetadataReference.MicrosoftAspNetCoreRouting(aspNetCoreRoutingVersion))
                    .Concat(NuGetMetadataReference.MicrosoftNetHttpHeaders(netHttpHeadersVersion));
    }
}
