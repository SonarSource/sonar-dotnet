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
    public class AssertionArgsShouldBePassedInCorrectOrderTest
    {
        [DataTestMethod]
        [DataRow("1.1.11")]
        [DataRow(Constants.NuGetLatestVersion)]
        [TestCategory("Rule")]
        public void AssertionArgsShouldBePassedInCorrectOrder_MsTest(string testFwkVersion)
        {
            Verifier.VerifyAnalyzer(@"TestCases\AssertionArgsShouldBePassedInCorrectOrder.MsTest.cs",
                new AssertionArgsShouldBePassedInCorrectOrder(),
                additionalReferences: NuGetMetadataReference.MSTestTestFramework(testFwkVersion).ToArray());
        }

        [DataTestMethod]
        [DataRow("2.5.7.10213")]
        [DataRow(Constants.NuGetLatestVersion)]
        [TestCategory("Rule")]
        public void AssertionArgsShouldBePassedInCorrectOrder_NUnit(string testFwkVersion)
        {
            Verifier.VerifyAnalyzer(@"TestCases\AssertionArgsShouldBePassedInCorrectOrder.NUnit.cs",
                new AssertionArgsShouldBePassedInCorrectOrder(),
                additionalReferences: NuGetMetadataReference.NUnit(testFwkVersion).ToArray());
        }

        [DataTestMethod]
        [DataRow("2.0.0")]
        [DataRow(Constants.NuGetLatestVersion)]
        [TestCategory("Rule")]
        public void AssertionArgsShouldBePassedInCorrectOrder_XUnit(string testFwkVersion)
        {
            Verifier.VerifyAnalyzer(@"TestCases\AssertionArgsShouldBePassedInCorrectOrder.Xunit.cs",
                new AssertionArgsShouldBePassedInCorrectOrder(),
                additionalReferences: NuGetMetadataReference.XunitFramework(testFwkVersion)
                    .Concat(FrameworkMetadataReference.Netstandard)
                    .ToArray());
        }
    }
}
