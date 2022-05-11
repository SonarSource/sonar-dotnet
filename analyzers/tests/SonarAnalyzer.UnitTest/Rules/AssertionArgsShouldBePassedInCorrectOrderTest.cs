/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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

using SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class AssertionArgsShouldBePassedInCorrectOrderTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder<AssertionArgsShouldBePassedInCorrectOrder>();

        [DataTestMethod]
        [DataRow("1.1.11")]
        [DataRow(Constants.NuGetLatestVersion)]
        public void AssertionArgsShouldBePassedInCorrectOrder_MsTest(string testFwkVersion) =>
            builder.AddPaths("AssertionArgsShouldBePassedInCorrectOrder.MsTest.cs")
                .AddReferences(NuGetMetadataReference.MSTestTestFramework(testFwkVersion))
                .Verify();

        [DataTestMethod]
        [DataRow("2.5.7.10213")]
        [DataRow(Constants.NuGetLatestVersion)]
        public void AssertionArgsShouldBePassedInCorrectOrder_NUnit(string testFwkVersion) =>
            builder.AddPaths("AssertionArgsShouldBePassedInCorrectOrder.NUnit.cs")
                .AddReferences(NuGetMetadataReference.NUnit(testFwkVersion))
                .Verify();

        [DataTestMethod]
        [DataRow("2.0.0")]
        [DataRow(Constants.NuGetLatestVersion)]
        public void AssertionArgsShouldBePassedInCorrectOrder_XUnit(string testFwkVersion) =>
            builder.AddPaths("AssertionArgsShouldBePassedInCorrectOrder.Xunit.cs")
                .AddReferences(NuGetMetadataReference.XunitFramework(testFwkVersion)
                                .Concat(NetStandardMetadataReference.Netstandard))
                .Verify();
    }
}
