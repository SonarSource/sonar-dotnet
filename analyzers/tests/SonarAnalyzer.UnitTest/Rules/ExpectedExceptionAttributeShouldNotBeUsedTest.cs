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
using SonarAnalyzer.UnitTest.MetadataReferences;
using SonarAnalyzer.UnitTest.TestFramework;
using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class ExpectedExceptionAttributeShouldNotBeUsedTest
    {
        [DataTestMethod]
        [DataRow("1.1.11")]
        [DataRow(Constants.NuGetLatestVersion)]
        [TestCategory("Rule")]
        public void ExpectedExceptionAttributeShouldNotBeUsed_MsTest_CS(string testFwkVersion) =>
            Verifier.VerifyAnalyzer(@"TestCases\ExpectedExceptionAttributeShouldNotBeUsed.MsTest.cs",
                                    new CS.ExpectedExceptionAttributeShouldNotBeUsed(),
                                    NuGetMetadataReference.MSTestTestFramework(testFwkVersion));

        [DataTestMethod]
        [DataRow("2.5.7.10213")]
        [DataRow("2.6.7")]
        [TestCategory("Rule")]
        public void ExpectedExceptionAttributeShouldNotBeUsed_NUnit_CS(string testFwkVersion) =>
            Verifier.VerifyAnalyzer(@"TestCases\ExpectedExceptionAttributeShouldNotBeUsed.NUnit.cs",
                                    new CS.ExpectedExceptionAttributeShouldNotBeUsed(),
                                    NuGetMetadataReference.NUnit(testFwkVersion));

        [DataTestMethod]
        [DataRow("3.0.0")]
        [DataRow(Constants.NuGetLatestVersion)]
        [TestCategory("Rule")]
        [Description("Starting with version 3.0.0 the attribute was removed.")]
        public void ExpectedExceptionAttributeShouldNotBeUsed_NUnit_NoIssue_CS(string testFwkVersion) =>
            Verifier.VerifyNoIssueReported(@"TestCases\ExpectedExceptionAttributeShouldNotBeUsed.NUnit.cs",
                                           new CS.ExpectedExceptionAttributeShouldNotBeUsed(),
                                           additionalReferences: NuGetMetadataReference.NUnit(testFwkVersion),
                                           checkMode: CompilationErrorBehavior.Ignore);

        [DataTestMethod]
        [DataRow("1.1.11")]
        [DataRow(Constants.NuGetLatestVersion)]
        [TestCategory("Rule")]
        public void ExpectedExceptionAttributeShouldNotBeUsed_MsTest_VB(string testFwkVersion) =>
            Verifier.VerifyAnalyzer(@"TestCases\ExpectedExceptionAttributeShouldNotBeUsed.MsTest.vb",
                                    new VB.ExpectedExceptionAttributeShouldNotBeUsed(),
                                    NuGetMetadataReference.MSTestTestFramework(testFwkVersion));

        [DataTestMethod]
        [DataRow("2.5.7.10213")]
        [DataRow("2.6.7")]
        [TestCategory("Rule")]
        public void ExpectedExceptionAttributeShouldNotBeUsed_NUnit_VB(string testFwkVersion) =>
            Verifier.VerifyAnalyzer(@"TestCases\ExpectedExceptionAttributeShouldNotBeUsed.NUnit.vb",
                                    new VB.ExpectedExceptionAttributeShouldNotBeUsed(),
                                    NuGetMetadataReference.NUnit(testFwkVersion));

        [DataTestMethod]
        [DataRow("3.0.0")]
        [DataRow(Constants.NuGetLatestVersion)]
        [TestCategory("Rule")]
        [Description("Starting with version 3.0.0 the attribute was removed.")]
        public void ExpectedExceptionAttributeShouldNotBeUsed_NUnit_NoIssue_VB(string testFwkVersion) =>
            Verifier.VerifyNoIssueReported(@"TestCases\ExpectedExceptionAttributeShouldNotBeUsed.NUnit.vb",
                                           new VB.ExpectedExceptionAttributeShouldNotBeUsed(),
                                           additionalReferences: NuGetMetadataReference.NUnit(testFwkVersion),
                                           checkMode: CompilationErrorBehavior.Ignore);
    }
}
