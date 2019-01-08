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

using System.Linq;
using csharp::SonarAnalyzer.Rules.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class TestMethodShouldNotBeIgnoredTest
    {
        [TestMethod]
        [TestCategory("Rule")]
        public void TestMethodShouldNotBeIgnored_MsTest_Legacy()
        {
            Verifier.VerifyAnalyzer(@"TestCases\TestMethodShouldNotBeIgnored.MsTest.cs",
                new TestMethodShouldNotBeIgnored(),
                additionalReferences: NuGetMetadataReference.MSTestTestFramework("1.1.11"),
                checkMode: CompilationErrorBehavior.Ignore); // IgnoreAttribute doesn't contain any reason param
        }

        [DataTestMethod]
        [DataRow("1.2.0")]
        [DataRow(Constants.NuGetLatestVersion)]
        [TestCategory("Rule")]
        public void TestMethodShouldNotBeIgnored_MsTest(string testFwkVersion)
        {
            Verifier.VerifyAnalyzer(@"TestCases\TestMethodShouldNotBeIgnored.MsTest.cs",
                new TestMethodShouldNotBeIgnored(),
                additionalReferences: NuGetMetadataReference.MSTestTestFramework(testFwkVersion));
        }

        [DataTestMethod]
        [DataRow("2.5.7.10213")]
        [DataRow("2.7.0")]
        [TestCategory("Rule")]
        public void TestMethodShouldNotBeIgnored_NUnit_V2(string testFwkVersion)
        {
            Verifier.VerifyAnalyzer(@"TestCases\TestMethodShouldNotBeIgnored.NUnit.V2.cs",
                new TestMethodShouldNotBeIgnored(),
                additionalReferences: NuGetMetadataReference.MSTestTestFrameworkV1
                    .Concat(NuGetMetadataReference.NUnit(testFwkVersion))
                    .ToArray());
        }

        [DataTestMethod]
        [DataRow("3.0.0")] // Ignore without reason no longer exist
        [DataRow(Constants.NuGetLatestVersion)]
        [TestCategory("Rule")]
        public void TestMethodShouldNotBeIgnored_NUnit(string testFwkVersion)
        {
            Verifier.VerifyAnalyzer(@"TestCases\TestMethodShouldNotBeIgnored.NUnit.cs",
                new TestMethodShouldNotBeIgnored(),
                additionalReferences: NuGetMetadataReference.MSTestTestFrameworkV1
                    .Concat(NuGetMetadataReference.NUnit(testFwkVersion))
                    .ToArray());
        }

        [DataTestMethod]
        [DataRow("2.0.0")]
        [DataRow(Constants.NuGetLatestVersion)]
        [TestCategory("Rule")]
        public void TestMethodShouldNotBeIgnored_Xunit(string testFwkVersion)
        {
            Verifier.VerifyAnalyzer(@"TestCases\TestMethodShouldNotBeIgnored.Xunit.cs",
                new TestMethodShouldNotBeIgnored(),
                additionalReferences: NuGetMetadataReference.MSTestTestFrameworkV1
                    .Concat(NuGetMetadataReference.XunitFramework(testFwkVersion))
                    .ToArray());
        }

        [TestMethod]
        [TestCategory("Rule")]
        public void TestMethodShouldNotBeIgnored_Xunit_v1()
        {
            Verifier.VerifyAnalyzer(@"TestCases\TestMethodShouldNotBeIgnored.Xunit.v1.cs",
                new TestMethodShouldNotBeIgnored(),
                additionalReferences: NuGetMetadataReference.MSTestTestFrameworkV1
                    .Concat(NuGetMetadataReference.XunitFrameworkV1)
                    .ToArray());
        }
    }
}
