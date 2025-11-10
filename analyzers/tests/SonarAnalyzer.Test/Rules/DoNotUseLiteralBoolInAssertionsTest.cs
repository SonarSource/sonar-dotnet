/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
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

namespace SonarAnalyzer.Test.Rules
{
    [TestClass]
    public class DoNotUseLiteralBoolInAssertionsTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder<DoNotUseLiteralBoolInAssertions>();

        [TestMethod]
        [DataRow(MsTest.Ver1_1)]
        [DataRow(MsTest.Ver3)]
        [DataRow(TestConstants.NuGetLatestVersion)]
        public void DoNotUseLiteralBoolInAssertions_MsTest(string testFwkVersion) =>
            builder.AddPaths("DoNotUseLiteralBoolInAssertions.MsTest.cs")
                .AddReferences(NuGetMetadataReference.MSTestTestFramework(testFwkVersion))
                .Verify();

        [TestMethod]
        [DataRow(NUnit.Ver25)]
        [DataRow(NUnit.Ver3Latest)] // Breaking changes in NUnit 4.0 would fail the test https://github.com/SonarSource/sonar-dotnet/issues/8409
        public void DoNotUseLiteralBoolInAssertions_NUnit(string testFwkVersion) =>
            builder.AddPaths("DoNotUseLiteralBoolInAssertions.NUnit.cs")
                .AddReferences(NuGetMetadataReference.NUnit(testFwkVersion))
                .Verify();

        [TestMethod]
        [DataRow("2.0.0")]
        [DataRow(XUnitVersions.Ver253)]
        public void DoNotUseLiteralBoolInAssertions_Xunit(string testFwkVersion) =>
            builder.AddPaths("DoNotUseLiteralBoolInAssertions.Xunit.cs")
                .AddReferences(NuGetMetadataReference.XunitFramework(testFwkVersion))
                .Verify();

        [TestMethod]
        public void DoNotUseLiteralBoolInAssertions_XunitV3() =>
            builder
                .AddPaths("DoNotUseLiteralBoolInAssertions.Xunit.cs")
                .AddPaths("DoNotUseLiteralBoolInAssertions.XunitV3.cs")
                .AddReferences(NuGetMetadataReference.XunitFrameworkV3(TestConstants.NuGetLatestVersion))
                .AddReferences(NuGetMetadataReference.SystemMemory(TestConstants.NuGetLatestVersion))
                .AddReferences(MetadataReferenceFacade.NetStandard)
                .AddReferences(MetadataReferenceFacade.SystemCollections)
                .Verify();
    }
}
