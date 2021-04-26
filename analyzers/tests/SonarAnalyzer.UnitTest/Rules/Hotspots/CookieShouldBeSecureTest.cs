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

using System.Collections.Generic;
#if NETFRAMEWORK
using System.IO;
#endif
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Common;
using SonarAnalyzer.UnitTest.MetadataReferences;
using SonarAnalyzer.UnitTest.TestFramework;
using CS = SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class CookieShouldBeSecureTest
    {
        [TestMethod]
        [TestCategory("Rule")]
        [TestCategory("Hotspot")]
        public void CookiesShouldBeSecure_Nancy() =>
            Verifier.VerifyAnalyzer(@"TestCases\Hotspots\CookieShouldBeSecure_Nancy.cs",
                                    new CS.CookieShouldBeSecure(AnalyzerConfiguration.AlwaysEnabled),
                                    AdditionalReferences);

#if NETFRAMEWORK // HttpCookie is not available on .Net Core

        private const string WebConfig = "Web.config";

        [TestMethod]
        [TestCategory("Rule")]
        [TestCategory("Hotspot")]
        public void CookiesShouldBeSecure() =>
            Verifier.VerifyAnalyzer(@"TestCases\Hotspots\CookieShouldBeSecure.cs",
                                    new CS.CookieShouldBeSecure(AnalyzerConfiguration.AlwaysEnabled),
                                    MetadataReferenceFacade.SystemWeb);

        [DataTestMethod]
        [DataRow(@"TestCases\WebConfig\CookieShouldBeSecure\SecureCookieConfig")]
        [DataRow(@"TestCases\WebConfig\CookieShouldBeSecure\Formatting")]
        [TestCategory("Rule")]
        [TestCategory("Hotspot")]
        public void CookiesShouldBeSecure_WithWebConfigValueSetToTrue(string root)
        {
            var webConfigPath = Path.Combine(root, WebConfig);
            Verifier.VerifyAnalyzer(@"TestCases\Hotspots\CookieShouldBeSecure_WithWebConfig.cs",
                                    new CS.CookieShouldBeSecure(AnalyzerConfiguration.AlwaysEnabled),
                                    MetadataReferenceFacade.SystemWeb,
                                    TestHelper.CreateSonarProjectConfig(root, TestHelper.CreateFilesToAnalyze(root, webConfigPath)));
        }

        [TestMethod]
        [TestCategory("Rule")]
        [TestCategory("Hotspot")]
        public void CookiesShouldBeSecure_WithWebConfigValueSetToFalse()
        {
            var root = @"TestCases\WebConfig\CookieShouldBeSecure\NonSecureCookieConfig";
            var webConfigPath = Path.Combine(root, WebConfig);
            Verifier.VerifyAnalyzer(@"TestCases\Hotspots\CookieShouldBeSecure.cs",
                                    new CS.CookieShouldBeSecure(AnalyzerConfiguration.AlwaysEnabled),
                                    MetadataReferenceFacade.SystemWeb,
                                    TestHelper.CreateSonarProjectConfig(root, TestHelper.CreateFilesToAnalyze(root, webConfigPath)));
        }

#else

        [TestMethod]
        [TestCategory("Rule")]
        [TestCategory("Hotspot")]
        public void CookiesShouldBeSecure_NetCore() =>
            Verifier.VerifyAnalyzer(@"TestCases\Hotspots\CookieShouldBeSecure_NetCore.cs",
                                    new CS.CookieShouldBeSecure(AnalyzerConfiguration.AlwaysEnabled),
                                    GetAdditionalReferences_NetCore());

        [TestMethod]
        [TestCategory("Rule")]
        [TestCategory("Hotspot")]
        public void CookiesShouldBeSecure_Net() =>
            Verifier.VerifyAnalyzerFromCSharp9Console(@"TestCases\Hotspots\CookieShouldBeSecure_Net.cs",
                                                      new CS.CookieShouldBeSecure(AnalyzerConfiguration.AlwaysEnabled),
                                                      GetAdditionalReferences_NetCore());

        private static IEnumerable<MetadataReference> GetAdditionalReferences_NetCore() =>
            NuGetMetadataReference.MicrosoftAspNetCoreHttpFeatures(Constants.NuGetLatestVersion);

#endif
        internal static IEnumerable<MetadataReference> AdditionalReferences =>
            NetStandardMetadataReference.Netstandard.Concat(NuGetMetadataReference.Nancy());
    }
}
