/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Common;
using CSharp = csharp::SonarAnalyzer.Rules.CSharp;
using VisualBasic = vbnet::SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class ExpandingArchivesTest
    {
        [TestMethod]
        [TestCategory("Rule")]
        public void ExpandingArchives_CS()
        {
            Verifier.VerifyAnalyzer(@"TestCases\ExpandingArchives.cs",
                new CSharp.ExpandingArchives(AnalyzerConfiguration.AlwaysEnabled),
                additionalReferences: GetAdditionalReferences());
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void ExpandingArchives_CS_Disabled()
        {
            Verifier.VerifyNoIssueReported(@"TestCases\ExpandingArchives.cs",
                new CSharp.ExpandingArchives(),
                additionalReferences: GetAdditionalReferences());
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void ExpandingArchives_VB()
        {
            Verifier.VerifyAnalyzer(@"TestCases\ExpandingArchives.vb",
                new VisualBasic.ExpandingArchives(AnalyzerConfiguration.AlwaysEnabled),
                additionalReferences: GetAdditionalReferences());
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void ExpandingArchives_VB_Disabled()
        {
            Verifier.VerifyNoIssueReported(@"TestCases\ExpandingArchives.vb",
                new VisualBasic.ExpandingArchives(),
                additionalReferences: GetAdditionalReferences());
        }

        private IEnumerable<MetadataReference> GetAdditionalReferences() =>
            FrameworkMetadataReference.SystemIOCompression
                .Concat(FrameworkMetadataReference.SystemIOCompressionFileSystem);
    }
}
