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

namespace SonarAnalyzer.Test.Rules;

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

    [TestMethod]
    public void AssertionArgsShouldBePassedInCorrectOrder_MsTest_Static() =>
        builder.WithTopLevelStatements().AddReferences(NuGetMetadataReference.MSTestTestFramework(Constants.NuGetLatestVersion)).AddSnippet("""
            using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;
            var str = "";
            AreEqual(str, ""); // Noncompliant
            """).Verify();

    [DataTestMethod]
    [DataRow("2.5.7.10213")]
    [DataRow("3.14.0")] // Breaking changes in NUnit 4.0 would fail the test https://github.com/SonarSource/sonar-dotnet/issues/8409
    public void AssertionArgsShouldBePassedInCorrectOrder_NUnit(string testFwkVersion) =>
        builder.AddPaths("AssertionArgsShouldBePassedInCorrectOrder.NUnit.cs")
            .AddReferences(NuGetMetadataReference.NUnit(testFwkVersion))
            .Verify();

    [TestMethod]
    public void AssertionArgsShouldBePassedInCorrectOrder_NUnit_Static() =>
        // Breaking changes in NUnit 4.0 would fail the test https://github.com/SonarSource/sonar-dotnet/issues/8409
        builder.WithTopLevelStatements().AddReferences(NuGetMetadataReference.NUnit("3.14.0")).AddSnippet("""
            using static NUnit.Framework.Assert;
            var str = "";
            AreEqual(str, ""); // Noncompliant
            """).Verify();

    [DataTestMethod]
    [DataRow(XUnitVersions.Ver2)]
    [DataRow(XUnitVersions.Ver253)]
    public void AssertionArgsShouldBePassedInCorrectOrder_XUnit(string testFwkVersion) =>
        builder.AddPaths("AssertionArgsShouldBePassedInCorrectOrder.Xunit.cs")
            .AddReferences(NuGetMetadataReference.XunitFramework(testFwkVersion)
                            .Concat(MetadataReferenceFacade.NetStandard))
            .Verify();

    [TestMethod]
    public void AssertionArgsShouldBePassedInCorrectOrder_XUnit_Static() =>
        builder.WithTopLevelStatements()
               .AddReferences(NuGetMetadataReference.XunitFramework(XUnitVersions.Ver253))
               .AddSnippet("""
                           using static Xunit.Assert;
                           var str = "";
                           Equal(str, ""); // Noncompliant
                           """).Verify();
}
