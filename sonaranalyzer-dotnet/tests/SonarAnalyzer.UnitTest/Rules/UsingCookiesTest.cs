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
extern alias vbnet;
using CSharp = csharp::SonarAnalyzer.Rules.CSharp;
using VisualBasic = vbnet::SonarAnalyzer.Rules.VisualBasic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using SonarAnalyzer.Common;
using SonarAnalyzer.UnitTest.MetadataReferences;
using SonarAnalyzer.UnitTest.TestFramework;

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
                new CSharp.UsingCookies(AnalyzerConfiguration.AlwaysEnabled),
                additionalReferences: GetAdditionalReferencesForNet46());

        [TestMethod]
        [TestCategory("Rule")]
        public void UsingCookies_CS_Disabled() =>
            Verifier.VerifyNoIssueReported(@"TestCases\UsingCookies_Net46.cs",
                new CSharp.UsingCookies(),
                additionalReferences: GetAdditionalReferencesForNet46());

        [TestMethod]
        [TestCategory("Rule")]
        public void UsingCookies_VB_Net46() =>
            Verifier.VerifyAnalyzer(@"TestCases\UsingCookies_Net46.vb",
                new VisualBasic.UsingCookies(AnalyzerConfiguration.AlwaysEnabled),
                additionalReferences: GetAdditionalReferencesForNet46());

        [TestMethod]
        [TestCategory("Rule")]
        public void UsingCookies_VB_Disabled() =>
            Verifier.VerifyNoIssueReported(@"TestCases\UsingCookies_Net46.vb",
                new VisualBasic.UsingCookies(),
                additionalReferences: GetAdditionalReferencesForNet46());

        private static IEnumerable<MetadataReference> GetAdditionalReferencesForNet46() =>
            FrameworkMetadataReference.SystemWeb;

#else
        [TestMethod]
        [TestCategory("Rule")]
        public void UsingCookies_CS_NetCore() =>
            Verifier.VerifyAnalyzer(@"TestCases\UsingCookies_NetCore.cs",
                new CSharp.UsingCookies(AnalyzerConfiguration.AlwaysEnabled),
                additionalReferences: GetAdditionalReferencesForNetCore(Constants.DotNetCore220Version));

        [TestMethod]
        [TestCategory("Rule")]
        public void UsingCookies_VB_NetCore() =>
            Verifier.VerifyAnalyzer(@"TestCases\UsingCookies_NetCore.vb",
                new VisualBasic.UsingCookies(AnalyzerConfiguration.AlwaysEnabled),
                additionalReferences: GetAdditionalReferencesForNetCore(Constants.DotNetCore220Version));

        private static IEnumerable<MetadataReference> GetAdditionalReferencesForNetCore(string packageVersion) =>
            NuGetMetadataReference.MicrosoftAspNetCoreHttpAbstractions(packageVersion)
                .Concat(NuGetMetadataReference.MicrosoftAspNetCoreHttpFeatures(packageVersion))
                .Concat(NuGetMetadataReference.MicrosoftExtensionsPrimitives(packageVersion));
#endif

    }
}
