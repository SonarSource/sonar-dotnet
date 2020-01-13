/*
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

using System.Collections.Generic;
using System.Linq;
using csharp::SonarAnalyzer.Rules.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Common;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class CookieShouldBeSecureTest
    {
        [TestMethod]
        [TestCategory("Rule")]
        public void CookiesShouldBeSecure()
        {
            Verifier.VerifyAnalyzer(@"TestCases\CookieShouldBeSecure.cs",
                new CookieShouldBeSecure(AnalyzerConfiguration.AlwaysEnabled),
                additionalReferences: FrameworkMetadataReference.SystemWeb);
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void CookiesShouldBeSecure_NetCore()
        {
            Verifier.VerifyAnalyzer(@"TestCases\CookieShouldBeSecure_NetCore.cs",
                new CookieShouldBeSecure(AnalyzerConfiguration.AlwaysEnabled),
                additionalReferences: GetAdditionalReferences_NetCore());
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void CookiesShouldBeSecure_Not_Enabled()
        {
            Verifier.VerifyNoIssueReported(@"TestCases\CookieShouldBeSecure.cs",
                new CookieShouldBeSecure(),
                additionalReferences: FrameworkMetadataReference.SystemWeb);
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void CookiesShouldBeSecure_Nancy()
        {
            Verifier.VerifyAnalyzer(@"TestCases\CookiesShouldBeSecure_Nancy.cs",
                new CookieShouldBeSecure(AnalyzerConfiguration.AlwaysEnabled),
                additionalReferences: NuGetMetadataReference.Nancy());
        }

        private static IEnumerable<MetadataReference> GetAdditionalReferences_NetCore() =>
            FrameworkMetadataReference.Netstandard
                .Concat(NuGetMetadataReference.MicrosoftAspNetCoreHttpFeatures(Constants.NuGetLatestVersion));
    }
}
