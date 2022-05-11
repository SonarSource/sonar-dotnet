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

#if NETFRAMEWORK
using System.IO;
#endif

using Microsoft.CodeAnalysis;
using SonarAnalyzer.Common;
using SonarAnalyzer.Rules.CSharp;
using SonarAnalyzer.UnitTest.MetadataReferences;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class CookieShouldBeSecureTest
    {
        [TestMethod]
        public void CookiesShouldBeSecure_Nancy() =>
            OldVerifier.VerifyAnalyzer(
                @"TestCases\Hotspots\CookieShouldBeSecure_Nancy.cs",
                new CookieShouldBeSecure(AnalyzerConfiguration.AlwaysEnabled),
                AdditionalReferences);

#if NETFRAMEWORK // HttpCookie is not available on .Net Core

        private const string WebConfig = "Web.config";

        [TestMethod]
        public void CookiesShouldBeSecure() =>
            OldVerifier.VerifyAnalyzer(
                @"TestCases\Hotspots\CookieShouldBeSecure.cs",
                new CookieShouldBeSecure(AnalyzerConfiguration.AlwaysEnabled),
                MetadataReferenceFacade.SystemWeb);

        [DataTestMethod]
        [DataRow(@"TestCases\WebConfig\CookieShouldBeSecure\SecureCookieConfig")]
        [DataRow(@"TestCases\WebConfig\CookieShouldBeSecure\Formatting")]
        public void CookiesShouldBeSecure_WithWebConfigValueSetToTrue(string root)
        {
            var webConfigPath = Path.Combine(root, WebConfig);
            OldVerifier.VerifyAnalyzer(
                @"TestCases\Hotspots\CookieShouldBeSecure_WithWebConfig.cs",
                new CookieShouldBeSecure(AnalyzerConfiguration.AlwaysEnabled),
                MetadataReferenceFacade.SystemWeb,
                TestHelper.CreateSonarProjectConfig(root, TestHelper.CreateFilesToAnalyze(root, webConfigPath)));
        }

        [DataTestMethod]
        [DataRow(@"TestCases\WebConfig\CookieShouldBeSecure\NonSecureCookieConfig")]
        [DataRow(@"TestCases\WebConfig\CookieShouldBeSecure\UnrelatedConfig")]
        [DataRow(@"TestCases\WebConfig\CookieShouldBeSecure\ConfigWithoutAttribute")]
        public void CookiesShouldBeSecure_WithWebConfigValueSetToFalse(string root)
        {
            var webConfigPath = Path.Combine(root, WebConfig);
            OldVerifier.VerifyAnalyzer(
                @"TestCases\Hotspots\CookieShouldBeSecure.cs",
                new CookieShouldBeSecure(AnalyzerConfiguration.AlwaysEnabled),
                MetadataReferenceFacade.SystemWeb,
                TestHelper.CreateSonarProjectConfig(root, TestHelper.CreateFilesToAnalyze(root, webConfigPath)));
        }

#else

        [TestMethod]
        public void CookiesShouldBeSecure_NetCore() =>
            OldVerifier.VerifyAnalyzer(
                @"TestCases\Hotspots\CookieShouldBeSecure_NetCore.cs",
                new CookieShouldBeSecure(AnalyzerConfiguration.AlwaysEnabled),
                GetAdditionalReferences_NetCore());

        [TestMethod]
        public void CookiesShouldBeSecure_CSharp9() =>
            OldVerifier.VerifyAnalyzerFromCSharp9Console(
                @"TestCases\Hotspots\CookieShouldBeSecure.CSharp9.cs",
                new CookieShouldBeSecure(AnalyzerConfiguration.AlwaysEnabled),
                GetAdditionalReferences_NetCore().Concat(NuGetMetadataReference.Nancy()));

        [TestMethod]
        public void CookiesShouldBeSecure_CSharp10() =>
            OldVerifier.VerifyAnalyzerFromCSharp10Console(
                @"TestCases\Hotspots\CookieShouldBeSecure.CSharp10.cs",
                new CookieShouldBeSecure(AnalyzerConfiguration.AlwaysEnabled),
                GetAdditionalReferences_NetCore().Concat(NuGetMetadataReference.Nancy()));

        private static IEnumerable<MetadataReference> GetAdditionalReferences_NetCore() =>
            NuGetMetadataReference.MicrosoftAspNetCoreHttpFeatures(Constants.NuGetLatestVersion);

#endif
        internal static IEnumerable<MetadataReference> AdditionalReferences =>
            NuGetMetadataReference.Nancy();
    }
}
