/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

namespace SonarAnalyzer.Test.Rules
{
    [TestClass]
    public class TestMethodShouldHaveCorrectSignatureTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder<TestMethodShouldHaveCorrectSignature>();

        [DataTestMethod]
        [DataRow("1.1.11")]
        [DataRow(Constants.NuGetLatestVersion)]
        public void TestMethodShouldHaveCorrectSignature_MsTest(string testFwkVersion) =>
            builder.AddPaths("TestMethodShouldHaveCorrectSignature.MsTest.cs")
                .AddReferences(NuGetMetadataReference.MSTestTestFramework(testFwkVersion))
                .Verify();

        [DataTestMethod]
        [DataRow("2.5.7.10213")]
        [DataRow(Constants.NuGetLatestVersion)]
        public void TestMethodShouldHaveCorrectSignature_NUnit(string testFwkVersion) =>
            builder.AddPaths("TestMethodShouldHaveCorrectSignature.NUnit.cs")
                .AddReferences(NuGetMetadataReference.NUnit(testFwkVersion))
                .Verify();

        [DataTestMethod]
        [DataRow("2.0.0")]
        [DataRow(Constants.NuGetLatestVersion)]
        public void TestMethodShouldHaveCorrectSignature_Xunit(string testFwkVersion) =>
            builder.AddPaths("TestMethodShouldHaveCorrectSignature.Xunit.cs")
                .AddReferences(NuGetMetadataReference.XunitFramework(testFwkVersion))
                .Verify();

        [TestMethod]
        public void TestMethodShouldHaveCorrectSignature_Xunit_Legacy() =>
            builder.AddPaths("TestMethodShouldHaveCorrectSignature.Xunit.Legacy.cs")
                .AddReferences(NuGetMetadataReference.XunitFrameworkV1)
                .Verify();

        [TestMethod]
        public void TestMethodShouldHaveCorrectSignature_MSTest_Miscellaneous() =>
            // Additional test cases e.g. partial classes, and methods with multiple faults.
            // We have to specify a test framework for the tests, but it doesn't really matter which
            // one, so we're using MSTest and only testing a single version.
            builder.AddPaths("TestMethodShouldHaveCorrectSignature.Misc.cs")
                .AddReferences(NuGetMetadataReference.MSTestTestFrameworkV1)
                .Verify();

#if NET

        [TestMethod]
        public void TestMethodShouldHaveCorrectSignature_CSharp9() =>
            builder.AddPaths("TestMethodShouldHaveCorrectSignature.CSharp9.cs")
                .AddReferences(NuGetMetadataReference.MSTestTestFrameworkV1
                    .Concat(NuGetMetadataReference.XunitFramework(Constants.NuGetLatestVersion))
                    .Concat(NuGetMetadataReference.NUnit(Constants.NuGetLatestVersion))
                    .ToArray())
                .WithOptions(ParseOptionsHelper.FromCSharp9)
                .Verify();

        [TestMethod]
        public void TestMethodShouldHaveCorrectSignature_CSharp11() =>
            builder.AddPaths("TestMethodShouldHaveCorrectSignature.CSharp11.cs")
                .AddReferences(NuGetMetadataReference.MSTestTestFrameworkV1)
                .WithOptions(ParseOptionsHelper.FromCSharp11)
                .Verify();

#endif

    }
}
