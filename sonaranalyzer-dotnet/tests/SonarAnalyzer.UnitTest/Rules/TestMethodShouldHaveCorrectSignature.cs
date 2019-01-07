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
using csharp::SonarAnalyzer.Rules.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class TestMethodShouldHaveCorrectSignatureTest
    {
        [DataTestMethod]
        [DataRow("1.1.11")]
        [DataRow(Constants.NuGetLatestVersion)]
        [TestCategory("Rule")]
        public void TestMethodShouldHaveCorrectSignature_MsTest(string testFwkVersion)
        {
            Verifier.VerifyAnalyzer(@"TestCases\TestMethodShouldHaveCorrectSignature.MsTest.cs",
                new TestMethodShouldHaveCorrectSignature(),
                additionalReferences: NuGetMetadataReference.MSTestTestFramework(testFwkVersion));
        }

        [DataTestMethod]
        [DataRow("2.5.7.10213")]
        [DataRow(Constants.NuGetLatestVersion)]
        [TestCategory("Rule")]
        public void TestMethodShouldHaveCorrectSignature_NUnit(string testFwkVersion)
        {
            Verifier.VerifyAnalyzer(@"TestCases\TestMethodShouldHaveCorrectSignature.NUnit.cs",
                new TestMethodShouldHaveCorrectSignature(),
                additionalReferences: NuGetMetadataReference.NUnit(testFwkVersion));
        }

        [DataTestMethod]
        [DataRow("2.0.0")]
        [DataRow(Constants.NuGetLatestVersion)]
        [TestCategory("Rule")]
        public void TestMethodShouldHaveCorrectSignature_Xunit(string testFwkVersion)
        {
            Verifier.VerifyAnalyzer(@"TestCases\TestMethodShouldHaveCorrectSignature.Xunit.cs",
                new TestMethodShouldHaveCorrectSignature(),
                additionalReferences: NuGetMetadataReference.XunitFramework(testFwkVersion));
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void TestMethodShouldHaveCorrectSignature_Xunit_Legacy()
        {
            Verifier.VerifyAnalyzer(@"TestCases\TestMethodShouldHaveCorrectSignature.Xunit.Legacy.cs",
                new TestMethodShouldHaveCorrectSignature(),
                additionalReferences: NuGetMetadataReference.XunitFrameworkV1);
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void TestMethodShouldHaveCorrectSignature_MSTest_Miscellaneous()
        {
            // Additional test cases e.g. partial classes, and methods with multiple faults.
            // We have to specify a test framework for the tests, but it doesn't really matter which
            // one, so we're using MSTest and only testing a single version.
            Verifier.VerifyAnalyzer(@"TestCases\TestMethodShouldHaveCorrectSignature.Misc.cs",
                new TestMethodShouldHaveCorrectSignature(),
                additionalReferences: NuGetMetadataReference.MSTestTestFrameworkV1);
        }
    }
}
