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
using csharp::SonarAnalyzer.Rules.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Common;
using SonarAnalyzer.UnitTest.MetadataReferences;
using SonarAnalyzer.UnitTest.TestFramework;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class CookieShouldBeHttpOnlyTest
    {
#if NETFRAMEWORK // The analyzed code is valid only for .Net Framework
        [TestMethod]
        [TestCategory("Rule")]
        public void CookiesShouldBeHttpOnly() =>
            Verifier.VerifyAnalyzer(@"TestCases\CookieShouldBeHttpOnly.cs",
                new CookieShouldBeHttpOnly(AnalyzerConfiguration.AlwaysEnabled),
                additionalReferences: MetadataReferenceFacade.GetSystemWeb());
#else
        [TestMethod]
        [TestCategory("Rule")]
        public void CookiesShouldBeHttpOnly_NetCore() =>
            Verifier.VerifyAnalyzer(@"TestCases\CookieShouldBeHttpOnly_NetCore.cs",
                new CookieShouldBeHttpOnly(AnalyzerConfiguration.AlwaysEnabled),
                additionalReferences: GetAdditionalReferences_NetCore());

        private static IEnumerable<MetadataReference> GetAdditionalReferences_NetCore() =>
            NuGetMetadataReference.MicrosoftAspNetCoreHttpFeatures(Constants.NuGetLatestVersion);
#endif

        [TestMethod]
        [TestCategory("Rule")]
        public void CookiesShouldBeHttpOnly_Nancy() =>
            Verifier.VerifyAnalyzer(@"TestCases\CookieShouldBeHttpOnly_Nancy.cs",
                new CookieShouldBeHttpOnly(AnalyzerConfiguration.AlwaysEnabled),
                additionalReferences: NuGetMetadataReference.Nancy());
    }
}
