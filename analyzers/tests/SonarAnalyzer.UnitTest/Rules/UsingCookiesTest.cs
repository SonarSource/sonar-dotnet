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
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Common;
using SonarAnalyzer.UnitTest.MetadataReferences;
using SonarAnalyzer.UnitTest.TestFramework;
using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class UsingCookies
    {
#if NETFRAMEWORK // HttpCookie is available only when targeting .Net Framework
        [TestMethod]
        [TestCategory("Rule")]
        public void UsingCookies_CS_Net46() =>
            Verifier.VerifyAnalyzer(@"TestCases\UsingCookies_Net46.cs",
                new CS.UsingCookies(AnalyzerConfiguration.AlwaysEnabled),
                GetAdditionalReferencesForNet46());

        [TestMethod]
        [TestCategory("Rule")]
        public void UsingCookies_CS_Disabled() =>
            Verifier.VerifyNoIssueReported(@"TestCases\UsingCookies_Net46.cs",
                new CS.UsingCookies(),
                additionalReferences: GetAdditionalReferencesForNet46());

        [TestMethod]
        [TestCategory("Rule")]
        public void UsingCookies_VB_Net46() =>
            Verifier.VerifyAnalyzer(@"TestCases\UsingCookies_Net46.vb",
                new VB.UsingCookies(AnalyzerConfiguration.AlwaysEnabled),
                GetAdditionalReferencesForNet46());

        [TestMethod]
        [TestCategory("Rule")]
        public void UsingCookies_VB_Disabled() =>
            Verifier.VerifyNoIssueReported(@"TestCases\UsingCookies_Net46.vb",
                new VB.UsingCookies(),
                additionalReferences: GetAdditionalReferencesForNet46());

        internal static IEnumerable<MetadataReference> GetAdditionalReferencesForNet46() =>
            FrameworkMetadataReference.SystemWeb;

#else
        [TestMethod]
        [TestCategory("Rule")]
        public void UsingCookies_CS_NetCore() =>
            Verifier.VerifyAnalyzer(@"TestCases\UsingCookies_NetCore.cs",
                new CS.UsingCookies(AnalyzerConfiguration.AlwaysEnabled),
                GetAdditionalReferencesForNetCore(Constants.DotNetCore220Version));

        [TestMethod]
        [TestCategory("Rule")]
        public void UsingCookies_VB_NetCore() =>
            Verifier.VerifyAnalyzer(@"TestCases\UsingCookies_NetCore.vb",
                new VB.UsingCookies(AnalyzerConfiguration.AlwaysEnabled),
                GetAdditionalReferencesForNetCore(Constants.DotNetCore220Version));

        internal static IEnumerable<MetadataReference> GetAdditionalReferencesForNetCore(string packageVersion) =>
            NuGetMetadataReference.MicrosoftAspNetCoreHttpAbstractions(packageVersion)
                .Concat(NuGetMetadataReference.MicrosoftAspNetCoreHttpFeatures(packageVersion))
                .Concat(NuGetMetadataReference.MicrosoftExtensionsPrimitives(packageVersion));
#endif

    }
}
