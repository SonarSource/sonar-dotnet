/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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
        public void UriShouldNotBeHardcoded_CS_General() =>
            builderCS.AddPaths("UriShouldNotBeHardcoded.cs").Verify();

        [TestMethod]
        public void UriShouldNotBeHardcoded_CS_Exceptions() =>
            builderCS
            .AddPaths("UriShouldNotBeHardcoded.Exceptions.cs")
            .AddReferences(MetadataReferenceFacade.SystemXml)
            .Verify();

#if NET

        [TestMethod]
        public void UriShouldNotBeHardcoded_CS_Latest() =>
            builderCS.AddPaths("UriShouldNotBeHardcoded.Latest.cs")
                .WithOptions(ParseOptionsHelper.CSharpLatest)
                .Verify();

#endif

#if NETFRAMEWORK // HttpContext is available only when targeting .Net Framework

        [DataTestMethod]
        [DataRow("3.0.20105.1")]
        [DataRow(Constants.NuGetLatestVersion)]
        public void UriShouldNotBeHardcoded_CS_VirtualPath_AspNet(string aspNetMvcVersion) =>
            builderCS
                .AddPaths("UriShouldNotBeHardcoded.AspNet.cs")
                .AddReferences(MetadataReferenceFacade.SystemWeb.Concat(NuGetMetadataReference.MicrosoftAspNetMvc(aspNetMvcVersion)))
                .Verify();

#endif

        [DataTestMethod]
        [DataRow("2.0.4", "2.0.3", "2.1.1")]
        [DataRow("2.2.0", "2.2.0", "2.2.0")]
        public void UriShouldNotBeHardcoded_CS_VirtualPath_AspNetCore(string aspNetCoreMvcVersion, string aspNetCoreRoutingVersion, string netHttpHeadersVersion) =>
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
