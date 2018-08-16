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

using System.Linq;
using csharp::SonarAnalyzer.Rules.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class TestMethodShouldContainAssertionTest
    {
        [DataTestMethod]
        [DataRow("1.1.11", "4.19.4")]
        [DataRow(Constants.NuGetLatestVersion, "4.19.4")]
        [TestCategory("Rule")]
        public void TestMethodShouldContainAssertion_MSTest(string testFwkVersion, string fluentVersion)
        {
            Verifier.VerifyAnalyzer(@"TestCases\TestMethodShouldContainAssertion.MsTest.cs",
                new TestMethodShouldContainAssertion(),
                additionalReferences: NuGetMetadataReference.MSTestTestFramework[testFwkVersion]
                    .Concat(NuGetMetadataReference.FluentAssertions[fluentVersion])
                    .ToArray());
        }

        [DataTestMethod]
        [DataRow("2.5.7.10213", "4.19.4")]
        [DataRow(Constants.NuGetLatestVersion, "4.19.4")]
        [TestCategory("Rule")]
        public void TestMethodShouldContainAssertion_NUnit(string testFwkVersion, string fluentVersion)
        {
            Verifier.VerifyAnalyzer(@"TestCases\TestMethodShouldContainAssertion.NUnit.cs",
                new TestMethodShouldContainAssertion(),
                additionalReferences: NuGetMetadataReference.NUnit[testFwkVersion]
                    .Concat(NuGetMetadataReference.FluentAssertions[fluentVersion])
                    .ToArray());
        }

        [DataTestMethod]
        [DataRow("2.0.0", "4.19.4")]
        [DataRow(Constants.NuGetLatestVersion, "4.19.4")]
        [TestCategory("Rule")]
        public void TestMethodShouldContainAssertion_Xunit(string testFwkVersion, string fluentVersion)
        {
            Verifier.VerifyAnalyzer(@"TestCases\TestMethodShouldContainAssertion.Xunit.cs",
                new TestMethodShouldContainAssertion(),
                additionalReferences: NuGetMetadataReference.XunitAssert[testFwkVersion]
                    .Concat(NuGetMetadataReference.XunitExtensibilityCore[testFwkVersion])
                    .Concat(NuGetMetadataReference.FluentAssertions[fluentVersion])
                    .ToArray());
        }
    }
}
