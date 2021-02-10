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

#if NET
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
#endif
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
        public void CookiesShouldBeSecure_Nancy() =>
            Verifier.VerifyAnalyzer(@"TestCases\Hotspots\CookieShouldBeSecure_Nancy.cs",
                new CS.CookieShouldBeSecure(AnalyzerConfiguration.AlwaysEnabled),
                NuGetMetadataReference.Nancy());

#if NETFRAMEWORK // HttpCookie is not available on .Net Core

        [TestMethod]
        [TestCategory("Rule")]
        public void CookiesShouldBeSecure() =>
            Verifier.VerifyAnalyzer(@"TestCases\Hotspots\CookieShouldBeSecure.cs",
                new CS.CookieShouldBeSecure(AnalyzerConfiguration.AlwaysEnabled),
                MetadataReferenceFacade.SystemWeb);

#else

        [TestMethod]
        [TestCategory("Rule")]
        public void CookiesShouldBeSecure_NetCore() =>
            Verifier.VerifyAnalyzer(@"TestCases\Hotspots\CookieShouldBeSecure_NetCore.cs",
                new CS.CookieShouldBeSecure(AnalyzerConfiguration.AlwaysEnabled),
                GetAdditionalReferences_NetCore());

        [TestMethod]
        [TestCategory("Rule")]
        public void CookiesShouldBeSecure_Net() =>
            Verifier.VerifyAnalyzerFromCSharp9Console(@"TestCases\Hotspots\CookieShouldBeSecure_Net.cs",
                                                      new CS.CookieShouldBeSecure(AnalyzerConfiguration.AlwaysEnabled),
                                                      GetAdditionalReferences_NetCore());

        private static IEnumerable<MetadataReference> GetAdditionalReferences_NetCore() =>
            NuGetMetadataReference.MicrosoftAspNetCoreHttpFeatures(Constants.NuGetLatestVersion);

#endif

    }
}
