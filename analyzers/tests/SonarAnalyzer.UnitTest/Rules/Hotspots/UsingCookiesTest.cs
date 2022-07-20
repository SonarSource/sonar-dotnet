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

using Microsoft.CodeAnalysis.CSharp;
using SonarAnalyzer.Common;
using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class UsingCookies
    {
        private readonly VerifierBuilder builderCS = new VerifierBuilder()
            .AddAnalyzer(() => new CS.UsingCookies(AnalyzerConfiguration.AlwaysEnabled))
            .WithBasePath(@"Hotspots\");

        private readonly VerifierBuilder builderVB = new VerifierBuilder()
            .AddAnalyzer(() => new VB.UsingCookies(AnalyzerConfiguration.AlwaysEnabled))
            .WithBasePath(@"Hotspots\");

#if NETFRAMEWORK // HttpCookie is available only when targeting .Net Framework
        [Ignore][TestMethod]
        public void UsingCookies_CS_Net46() =>
            builderCS
                .AddPaths("UsingCookies_Net46.cs")
                .AddReferences(GetAdditionalReferencesForNet46())
                .Verify();

        [Ignore][TestMethod]
        public void UsingCookies_VB_Net46() =>
            builderVB
                .AddPaths("UsingCookies_Net46.vb")
                .AddReferences(GetAdditionalReferencesForNet46())
                .Verify();

        internal static IEnumerable<MetadataReference> GetAdditionalReferencesForNet46() =>
            FrameworkMetadataReference.SystemWeb;

#else
        [Ignore][TestMethod]
        public void UsingCookies_CS_NetCore() =>
            builderCS
                .AddPaths("UsingCookies_NetCore.cs")
                .AddReferences(GetAspNetCoreReferences(Constants.DotNetCore220Version))
                .Verify();

        [Ignore][TestMethod]
        public void UsingCookies_CS_NetCore_DotnetCoreLatest() =>
            builderCS
                .AddPaths("UsingCookies_NetCore.cs")
                .AddReferences(GetAspNetCoreReferences(Constants.NuGetLatestVersion))
                .Verify();

        [Ignore][TestMethod]
        public void UsingCookies_CSharp10_DotnetCoreLatest() =>
            builderCS
                .AddPaths("UsingCookies_NetCore.CSharp10.cs")
                .AddReferences(GetAspNetCoreReferences(Constants.NuGetLatestVersion))
                .WithLanguageVersion(LanguageVersion.CSharp10)
                .Verify();

        [Ignore][TestMethod]
        public void UsingCookies_VB_NetCore_DotnetCoreLatest() =>
            builderVB
                .AddPaths("UsingCookies_NetCore.vb")
                .AddReferences(GetAspNetCoreReferences(Constants.NuGetLatestVersion))
                .Verify();

        internal static IEnumerable<MetadataReference> GetAspNetCoreReferences(string packageVersion) =>
            NuGetMetadataReference.MicrosoftAspNetCoreHttpAbstractions(packageVersion)
                .Concat(NuGetMetadataReference.MicrosoftAspNetCoreHttpFeatures(packageVersion))
                .Concat(NuGetMetadataReference.MicrosoftExtensionsPrimitives(packageVersion));
#endif
    }
}
