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

using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Common;
using SonarAnalyzer.Rules.CSharp;
using SonarAnalyzer.UnitTest.TestFramework;
using VB = SonarAnalyzer.Rules.VisualBasic;

#if NET
using SonarAnalyzer.UnitTest.MetadataReferences;
#endif

namespace SonarAnalyzer.UnitTest.Rules.Hotspots
{
    [TestClass]
    public class LooseFilePermissionsTest
    {
        [TestMethod]
        [TestCategory("Rule")]
        public void LooseFilePermissions_Windows_CS() =>
            Verifier.VerifyAnalyzer(@"TestCases\Hotspots\LooseFilePermissions.Windows.cs", new LooseFilePermissions(AnalyzerConfiguration.AlwaysEnabled));

        [TestMethod]
        [TestCategory("Rule")]
        public void LooseFilePermissions_Windows_VB() =>
            Verifier.VerifyAnalyzer(@"TestCases\Hotspots\LooseFilePermissions.Windows.vb", new VB.LooseFilePermissions(AnalyzerConfiguration.AlwaysEnabled));

#if NET
        [TestMethod]
        [TestCategory("Rule")]
        public void LooseFilePermissions_Windows_CSharp9() =>
            Verifier.VerifyNonConcurrentAnalyzerFromCSharp9Console(@"TestCases\Hotspots\LooseFilePermissions.Windows.CSharp9.cs", new LooseFilePermissions(AnalyzerConfiguration.AlwaysEnabled));

        [TestMethod]
        [TestCategory("Rule")]
        public void LooseFilePermissions_Unix_CS() =>
            Verifier.VerifyAnalyzer(@"TestCases\Hotspots\LooseFilePermissions.Unix.cs",
                                    new LooseFilePermissions(AnalyzerConfiguration.AlwaysEnabled),
                                    NuGetMetadataReference.MonoPosixNetStandard());

        [TestMethod]
        [TestCategory("Rule")]
        public void LooseFilePermissions_Unix_VB() =>
            Verifier.VerifyAnalyzer(@"TestCases\Hotspots\LooseFilePermissions.Unix.vb",
                                    new VB.LooseFilePermissions(AnalyzerConfiguration.AlwaysEnabled),
                                    NuGetMetadataReference.MonoPosixNetStandard());
#endif
    }
}
