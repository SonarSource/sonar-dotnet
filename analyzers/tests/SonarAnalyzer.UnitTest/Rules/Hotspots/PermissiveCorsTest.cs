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

using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Common;
using SonarAnalyzer.Rules.CSharp;
using SonarAnalyzer.UnitTest.MetadataReferences;
using SonarAnalyzer.UnitTest.TestFramework;

#if NETFRAMEWORK
using System.Linq;
#endif

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class PermissiveCorsTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder()
            .AddAnalyzer(() => new PermissiveCors(AnalyzerConfiguration.AlwaysEnabled))
            .WithBasePath(@"Hotspots\");
#if NET
        [TestMethod]
        public void PermissiveCors_CS() =>
            builder
                .AddPaths("PermissiveCors.Net.cs")
                .AddReferences(AdditionalReferences)
                .WithLanguageVersion(LanguageVersion.CSharp9)
                .Verify();

        [TestMethod]
        public void PermissiveCors_CSharp10() =>
            builder
                .AddPaths("PermissiveCors.CSharp10.cs")
                .AddReferences(AdditionalReferences)
                .WithLanguageVersion(LanguageVersion.CSharp10)
                .Verify();

        [TestMethod]
        public void PermissiveCors_CSharpPreview() =>
            builder
                .AddPaths("PermissiveCors.CSharp.Preview.cs")
                .AddReferences(AdditionalReferences)
                .WithLanguageVersion(LanguageVersion.Preview)
                .Verify();

        internal static IEnumerable<MetadataReference> AdditionalReferences =>
            new[]
            {
                AspNetCoreMetadataReference.MicrosoftAspNetCoreCors,
                AspNetCoreMetadataReference.MicrosoftAspNetCoreHttpAbstractions,
                AspNetCoreMetadataReference.MicrosoftAspNetCoreHttpFeatures,
                AspNetCoreMetadataReference.MicrosoftAspNetCoreMvc,
                AspNetCoreMetadataReference.MicrosoftAspNetCoreMvcAbstractions,
                AspNetCoreMetadataReference.MicrosoftAspNetCoreMvcCore,
                AspNetCoreMetadataReference.MicrosoftAspNetCoreMvcViewFeatures,
                CoreMetadataReference.MicrosoftExtensionsDependencyInjectionAbstractions,
                CoreMetadataReference.MicrosoftExtensionsPrimitives,
                CoreMetadataReference.MicrosoftNetHttpHeadersHeaderNames
            };
#else
        [TestMethod]
        public void PermissiveCors_AspNet_WebApi() =>
            OldVerifier.VerifyAnalyzer(@"TestCases\Hotspots\PermissiveCors.NetFramework.cs",
                                    new PermissiveCors(AnalyzerConfiguration.AlwaysEnabled),
                                    ParseOptionsHelper.FromCSharp9,
                                    AdditionalReferences);

        private static IEnumerable<MetadataReference> AdditionalReferences =>
            NuGetMetadataReference.MicrosoftNetHttpHeaders(Constants.NuGetLatestVersion)
                                  .Concat(NuGetMetadataReference.MicrosoftAspNetMvc(Constants.NuGetLatestVersion))
                                  .Concat(NuGetMetadataReference.MicrosoftAspNetWebApiCors(Constants.NuGetLatestVersion))
                                  .Concat(NuGetMetadataReference.MicrosoftNetWebApiCore(Constants.NuGetLatestVersion))
                                  .Concat(FrameworkMetadataReference.SystemWeb)
                                  .Concat(FrameworkMetadataReference.SystemNetHttp);
#endif
    }
}
