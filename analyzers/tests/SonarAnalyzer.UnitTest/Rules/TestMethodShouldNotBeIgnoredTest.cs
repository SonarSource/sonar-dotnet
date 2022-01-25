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

using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Rules.CSharp;
using SonarAnalyzer.UnitTest.MetadataReferences;
using SonarAnalyzer.UnitTest.TestFramework;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class TestMethodShouldNotBeIgnoredTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder<TestMethodShouldNotBeIgnored>().AddReferences(NuGetMetadataReference.MSTestTestFrameworkV1);

        [TestMethod]
        public void TestMethodShouldNotBeIgnored_MsTest_Legacy() =>
            builder.AddPaths("TestMethodShouldNotBeIgnored.MsTest.cs")
                .WithErrorBehavior(CompilationErrorBehavior.Ignore)    // IgnoreAttribute doesn't contain any reason param
                .Verify();

        [DataTestMethod]
        [DataRow("1.2.0")]
        [DataRow(Constants.NuGetLatestVersion)]
        public void TestMethodShouldNotBeIgnored_MsTest(string testFwkVersion) =>
            new VerifierBuilder<TestMethodShouldNotBeIgnored>()
                .AddPaths("TestMethodShouldNotBeIgnored.MsTest.cs")
                .AddReferences(NuGetMetadataReference.MSTestTestFramework(testFwkVersion)).Verify();

        [DataTestMethod]
        [DataRow("2.5.7.10213")]
        [DataRow("2.7.0")]
        public void TestMethodShouldNotBeIgnored_NUnit_V2(string testFwkVersion) =>
            builder.AddPaths("TestMethodShouldNotBeIgnored.NUnit.V2.cs")
                .AddReferences(NuGetMetadataReference.NUnit(testFwkVersion))
                .Verify();

        [DataTestMethod]
        [DataRow("3.0.0")] // Ignore without reason no longer exist
        [DataRow(Constants.NuGetLatestVersion)]
        public void TestMethodShouldNotBeIgnored_NUnit(string testFwkVersion) =>
            builder.AddPaths("TestMethodShouldNotBeIgnored.NUnit.cs").AddReferences(NuGetMetadataReference.NUnit(testFwkVersion)).Verify();

        [DataTestMethod]
        [DataRow("2.0.0")]
        [DataRow(Constants.NuGetLatestVersion)]
        public void TestMethodShouldNotBeIgnored_Xunit(string testFwkVersion) =>
            builder.AddPaths("TestMethodShouldNotBeIgnored.Xunit.cs").AddReferences(NuGetMetadataReference.XunitFramework(testFwkVersion)).Verify();

        [TestMethod]
        public void TestMethodShouldNotBeIgnored_Xunit_v1() =>
            builder.AddPaths("TestMethodShouldNotBeIgnored.Xunit.v1.cs").AddReferences(NuGetMetadataReference.XunitFrameworkV1).Verify();

#if NET

        [TestMethod]
        public void TestMethodShouldNotBeIgnored_CSharp9() =>
            builder.AddPaths("TestMethodShouldNotBeIgnored.CSharp9.cs")
                .AddReferences(NuGetMetadataReference.XunitFrameworkV1)
                .AddReferences(NuGetMetadataReference.NUnit(Constants.NuGetLatestVersion))
                .WithOptions(ParseOptionsHelper.FromCSharp9)
                .Verify();

#endif

    }
}
