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
using SonarAnalyzer.Test.Common;

namespace SonarAnalyzer.Test.Rules
{
    [TestClass]
    public class DoNotUseLiteralBoolInAssertionsTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder<DoNotUseLiteralBoolInAssertions>();

        [DataTestMethod]
        [DataRow("1.1.11")]
        [DataRow(Constants.NuGetLatestVersion)]
        public void DoNotUseLiteralBoolInAssertions_MsTest(string testFwkVersion) =>
            builder.AddPaths("DoNotUseLiteralBoolInAssertions.MsTest.cs")
                .AddReferences(NuGetMetadataReference.MSTestTestFramework(testFwkVersion))
                .Verify();

        [DataTestMethod]
        [DataRow("2.5.7.10213")]
        [DataRow("3.14.0")] // Breaking changes in NUnit 4.0 would fail the test https://github.com/SonarSource/sonar-dotnet/issues/8409
        public void DoNotUseLiteralBoolInAssertions_NUnit(string testFwkVersion) =>
            builder.AddPaths("DoNotUseLiteralBoolInAssertions.NUnit.cs")
                .AddReferences(NuGetMetadataReference.NUnit(testFwkVersion))
                .Verify();

        [DataTestMethod]
        [DataRow("2.0.0")]
        [DataRow(XUnitVersions.Ver253)]
        public void DoNotUseLiteralBoolInAssertions_Xunit(string testFwkVersion) =>
            builder.AddPaths("DoNotUseLiteralBoolInAssertions.Xunit.cs")
                .AddReferences(NuGetMetadataReference.XunitFramework(testFwkVersion))
                .Verify();
    }
}
