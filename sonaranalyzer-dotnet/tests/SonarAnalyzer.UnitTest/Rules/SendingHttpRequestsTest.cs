/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using csharp::SonarAnalyzer.Rules.CSharp;
using Microsoft.CodeAnalysis;
using System.Linq;
using System.Collections.Generic;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class SendingHttpRequestsTest
    {
        private readonly IEnumerable<MetadataReference> WebComponentReferences =
            FrameworkMetadataReference.SystemNetHttp
            .Concat(FrameworkMetadataReference.Netstandard)
            .Concat(NuGetMetadataReference.Create("RestSharp", Constants.NuGetLatestVersion));

        [TestMethod]
        [TestCategory("Rule")]
        public void SendingHttpRequests_CS()
        {
            Verifier.VerifyAnalyzer(@"TestCases\SendingHttpRequests.cs",
                new SendingHttpRequests(new TestAnalyzerConfiguration(null, "S4825")),
                additionalReferences: WebComponentReferences);
        }

        [TestMethod]
        [TestCategory("Rule")]
        [TestCategory("Hotspot")]
        public void SendingHttpRequests_VB()
        {
            Verifier.VerifyAnalyzer(@"TestCases\SendingHttpRequests.vb",
                new SonarAnalyzer.Rules.VisualBasic.SendingHttpRequests(new TestAnalyzerConfiguration(null, "S4825")),
                additionalReferences: WebComponentReferences);
        }

        [TestMethod]
        [TestCategory("Rule")]
        [TestCategory("Hotspot")]
        public void SendingHttpRequests_CS_RuleDisabled()
        {
            Verifier.VerifyNoIssueReported(@"TestCases\SendingHttpRequests.cs",
                new SendingHttpRequests(),
                additionalReferences: WebComponentReferences);
        }

        [TestMethod]
        [TestCategory("Rule")]
        [TestCategory("Hotspot")]
        public void SendingHttpRequests_VB_RuleDisabled()
        {
            Verifier.VerifyNoIssueReported(@"TestCases\SendingHttpRequests.vb",
                new SonarAnalyzer.Rules.VisualBasic.SendingHttpRequests(),
                additionalReferences: WebComponentReferences);
        }
    }
}

