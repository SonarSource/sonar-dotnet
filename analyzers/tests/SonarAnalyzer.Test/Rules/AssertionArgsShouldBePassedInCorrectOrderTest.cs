/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using SonarAnalyzer.CSharp.Rules;
using SonarAnalyzer.Test.Common;

using static SonarAnalyzer.TestFramework.MetadataReferences.NugetPackageVersions;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class AssertionArgsShouldBePassedInCorrectOrderTest
{
    private readonly VerifierBuilder builder = new VerifierBuilder<AssertionArgsShouldBePassedInCorrectOrder>();

    [TestMethod]
    [DataRow(MsTest.Ver1_1)]
    [DataRow(MsTest.Ver3)]
    [DataRow(TestConstants.NuGetLatestVersion)]
    public void AssertionArgsShouldBePassedInCorrectOrder_MsTest(string testFwkVersion) =>
        builder.AddPaths("AssertionArgsShouldBePassedInCorrectOrder.MsTest.cs")
            .AddReferences(NuGetMetadataReference.MSTestTestFramework(testFwkVersion))
            .Verify();

    [TestMethod]
    public void AssertionArgsShouldBePassedInCorrectOrder_MsTest_Static() =>
        builder.WithTopLevelStatements().AddReferences(NuGetMetadataReference.MSTestTestFramework(TestConstants.NuGetLatestVersion)).AddSnippet("""
            using static Microsoft.VisualStudio.TestTools.UnitTesting.Assert;
            var str = "";
            AreEqual(str, ""); // Noncompliant
            """).Verify();

    [TestMethod]
    [DataRow(NUnit.Ver25)]
    [DataRow(NUnit.Ver3Latest)] // Breaking changes in NUnit 4.0 would fail the test https://github.com/SonarSource/sonar-dotnet/issues/8409
    public void AssertionArgsShouldBePassedInCorrectOrder_NUnit(string testFwkVersion) =>
        builder.AddPaths("AssertionArgsShouldBePassedInCorrectOrder.NUnit.cs")
            .AddReferences(NuGetMetadataReference.NUnit(testFwkVersion))
            .Verify();

    [TestMethod]
    public void AssertionArgsShouldBePassedInCorrectOrder_NUnit_Static() =>
        // Breaking changes in NUnit 4.0 would fail the test https://github.com/SonarSource/sonar-dotnet/issues/8409
        builder.WithTopLevelStatements().AddReferences(NuGetMetadataReference.NUnit(NUnit.Ver3Latest)).AddSnippet("""
            using static NUnit.Framework.Assert;
            var str = "";
            AreEqual(str, ""); // Noncompliant
            """).Verify();

    [TestMethod]
    [DataRow(XUnitVersions.Ver2)]
    [DataRow(XUnitVersions.Ver253)]
    public void AssertionArgsShouldBePassedInCorrectOrder_XUnit(string testFwkVersion) =>
        builder.AddPaths("AssertionArgsShouldBePassedInCorrectOrder.Xunit.cs")
            .AddReferences(NuGetMetadataReference.XunitFramework(testFwkVersion)
                            .Concat(MetadataReferenceFacade.NetStandard))
            .Verify();

    [TestMethod]
    public void AssertionArgsShouldBePassedInCorrectOrder_XUnitV3() =>
        builder
            .AddPaths("AssertionArgsShouldBePassedInCorrectOrder.Xunit.cs")
            .AddPaths("AssertionArgsShouldBePassedInCorrectOrder.XunitV3.cs")
            .AddReferences(NuGetMetadataReference.XunitFrameworkV3())
            .AddReferences(NuGetMetadataReference.SystemMemory(TestConstants.NuGetLatestVersion))
            .AddReferences(MetadataReferenceFacade.NetStandard)
            .AddReferences(MetadataReferenceFacade.SystemCollections)
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
