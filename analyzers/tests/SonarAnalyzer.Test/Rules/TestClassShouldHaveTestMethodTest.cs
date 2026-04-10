/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
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
using static SonarAnalyzer.TestFramework.MetadataReferences.NugetPackageVersions;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class TestClassShouldHaveTestMethodTest
{
    private readonly VerifierBuilder builder = new VerifierBuilder<TestClassShouldHaveTestMethod>();

    [TestMethod]
    [DataRow(NUnit.Ver25)]
    [DataRow(NUnit.Ver3Latest)] // Breaking changes in NUnit 4.0 would fail the test https://github.com/SonarSource/sonar-dotnet/issues/8409
    public void TestClassShouldHaveTestMethod_NUnit(string testFwkVersion) =>
        builder
            .AddPaths("TestClassShouldHaveTestMethod.NUnit.cs")
            .AddReferences(NuGetMetadataReference.NUnit(testFwkVersion))
            .Verify();

    [TestMethod]
    public void TestClassShouldHaveTestMethod_NUnit4() =>
        builder
            .AddPaths("TestClassShouldHaveTestMethod.NUnit4.cs")
            .AddReferences(NuGetMetadataReference.NUnit(NUnit.Ver4))
            .Verify();

    [TestMethod]
    [DataRow("3.0.0")]
    [DataRow(TestConstants.NuGetLatestVersion)]
    public void TestClassShouldHaveTestMethod_NUnit3(string testFwkVersion) =>
        builder
            .AddPaths("TestClassShouldHaveTestMethod.NUnit3.cs")
            .AddReferences(NuGetMetadataReference.NUnit(testFwkVersion))
            .Verify();

    [TestMethod]
    [DataRow(MsTest.Ver1_1)]
    [DataRow(MsTest.Ver3)]
    [DataRow(Latest)]
    public void TestClassShouldHaveTestMethod_MSTest(string testFwkVersion) =>
        builder
            .AddPaths("TestClassShouldHaveTestMethod.MsTest.cs")
            .AddReferences(NuGetMetadataReference.MSTestTestFramework(testFwkVersion))
            .Verify();

    [TestMethod]
    public void TestClassShouldHaveTestMethod_Latest() =>
        builder
            .AddPaths("TestClassShouldHaveTestMethod.Latest.cs")
            .WithOptions(LanguageOptions.CSharpLatest)
            .AddReferences(NuGetMetadataReference.MSTestTestFramework(TestConstants.NuGetLatestVersion))
            .AddReferences(NuGetMetadataReference.NUnit(TestConstants.NuGetLatestVersion))
            .Verify();

    [TestMethod]
    public void TestClassShouldHaveTestMethod_NUnit4_AliasedNamespace() =>
        builder.AddReferences(NuGetMetadataReference.NUnit(NUnit.Ver4)).AddSnippet("""
            using NUnit.Framework;
            namespace Aliased
            {
                using Assert = NUnit.Framework.Legacy.ClassicAssert;
                [TestFixture]
                class ClassTest1 // Noncompliant
                {
                }
                [TestFixture]
                public class ClassTest2
                {
                    [TestCaseSource("DivideCases")]
                    public void DivideTest(int n, int d, int q)
                    {
                        Assert.AreEqual(q, n / d);
                    }
                    static object[] DivideCases = { new object[] { 12, 3, 4 } };
                }
            }
            """).Verify();
}
