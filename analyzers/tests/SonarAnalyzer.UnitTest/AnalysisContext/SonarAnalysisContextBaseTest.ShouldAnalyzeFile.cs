/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

namespace SonarAnalyzer.UnitTest.AnalysisContext;

public partial class SonarAnalysisContextBaseTest
{
    [DataTestMethod]
    [DataRow("Foo", new string[] { "Foo" }, ProjectType.Product, false)]
    [DataRow("Foo", new string[] { "NotFoo" }, ProjectType.Product, true)]
    [DataRow("Foo", new string[] { "Foo" }, ProjectType.Test, true)]
    [DataRow("Foo", new string[] { "NotFoo" }, ProjectType.Test, true)]
    public void ShouldAnalyzeFile_Exclusions_ReturnExpected(string filePath, string[] exclusions, ProjectType projectType, bool shouldAnalyze) =>
        AssertShouldAnalyzeFile(filePath, projectType, shouldAnalyze, exclusions: exclusions);

    [DataTestMethod]
    [DataRow("Foo", new string[] { "Foo" }, ProjectType.Product, false)]
    [DataRow("Foo", new string[] { "NotFoo" }, ProjectType.Product, true)]
    [DataRow("Foo", new string[] { "Foo" }, ProjectType.Test, true)]
    [DataRow("Foo", new string[] { "NotFoo" }, ProjectType.Test, true)]
    public void ShouldAnalyzeFile_GlobalExclusions_ReturnExpected(string filePath, string[] globalExclusions, ProjectType projectType, bool shouldAnalyze) =>
        AssertShouldAnalyzeFile(filePath, projectType, shouldAnalyze, globalExclusions: globalExclusions);

    [DataTestMethod]
    [DataRow("Foo", new string[] { "Foo" }, ProjectType.Product, true)]
    [DataRow("Foo", new string[] { "NotFoo" }, ProjectType.Product, true)]
    [DataRow("Foo", new string[] { "Foo" }, ProjectType.Test, false)]
    [DataRow("Foo", new string[] { "NotFoo" }, ProjectType.Test, true)]
    public void ShouldAnalyzeFile_TestExclusions_ReturnExpected(string filePath, string[] testExclusions, ProjectType projectType, bool shouldAnalyze) =>
        AssertShouldAnalyzeFile(filePath, projectType, shouldAnalyze, testExclusions: testExclusions);

    [DataTestMethod]
    [DataRow("Foo", new string[] { "Foo" }, ProjectType.Product, true)]
    [DataRow("Foo", new string[] { "NotFoo" }, ProjectType.Product, true)]
    [DataRow("Foo", new string[] { "Foo" }, ProjectType.Test, false)]
    [DataRow("Foo", new string[] { "NotFoo" }, ProjectType.Test, true)]
    public void ShouldAnalyzeFile_GlobalTestExclusions_ReturnExpected(string filePath, string[] globalTestExclusions, ProjectType projectType, bool shouldAnalyze) =>
        AssertShouldAnalyzeFile(filePath, projectType, shouldAnalyze, globalTestExclusions: globalTestExclusions);

    [DataTestMethod]
    [DataRow("Foo", new string[] { "Foo" }, ProjectType.Product, true)]
    [DataRow("Foo", new string[] { "NotFoo" }, ProjectType.Product, false)]
    [DataRow("Foo", new string[] { "Foo" }, ProjectType.Test, true)]
    [DataRow("Foo", new string[] { "NotFoo" }, ProjectType.Test, true)]
    public void ShouldAnalyzeFile_Inclusions_ReturnExpected(string filePath, string[] inclusions, ProjectType projectType, bool shouldAnalyze) =>
        AssertShouldAnalyzeFile(filePath, projectType, shouldAnalyze, inclusions: inclusions);

    [DataTestMethod]
    [DataRow("Foo", new string[] { "Foo" }, ProjectType.Product, true)]
    [DataRow("Foo", new string[] { "NotFoo" }, ProjectType.Product, true)]
    [DataRow("Foo", new string[] { "Foo" }, ProjectType.Test, true)]
    [DataRow("Foo", new string[] { "NotFoo" }, ProjectType.Test, false)]
    public void ShouldAnalyzeFile_TestInclusions_ReturnExpected(string filePath, string[] testInclusions, ProjectType projectType, bool shouldAnalyze) =>
        AssertShouldAnalyzeFile(filePath, projectType, shouldAnalyze, testInclusions: testInclusions);

    [DataTestMethod]
    [DataRow("Foo", new string[] { "Foo" }, new string[] { "Foo" }, ProjectType.Product, false)]
    [DataRow("Foo", new string[] { "NotFoo" }, new string[] { "Foo" }, ProjectType.Product, false)]
    [DataRow("Foo", new string[] { "Foo" }, new string[] { "NotFoo" }, ProjectType.Product, true)]
    [DataRow("Foo", new string[] { "NotFoo" }, new string[] { "NotFoo" }, ProjectType.Product, false)]
    public void ShouldAnalyzeFile_MixedInput_ReturnExpected(string filePath, string[] inclusions, string[] exclusions, ProjectType projectType, bool shouldAnalyze) =>
        AssertShouldAnalyzeFile(filePath, projectType, shouldAnalyze, inclusions: inclusions, exclusions: exclusions);

    private void AssertShouldAnalyzeFile(
        string filePath,
        ProjectType projectType,
        bool shouldAnalyze,
        string[] exclusions = null,
        string[] inclusions = null,
        string[] globalExclusions = null,
        string[] testExclusions = null,
        string[] testInclusions = null,
        string[] globalTestExclusions = null)
    {
        var compilation = new SnippetCompiler("// Nothing to see here", TestHelper.ProjectTypeReference(projectType)).SemanticModel.Compilation;
        var sonarLintXml = AnalysisScaffolding.CreateSonarLintXml(
            TestContext,
            exclusions: exclusions,
            inclusions: inclusions,
            globalExclusions: globalExclusions,
            testExclusions: testExclusions,
            testInclusions: testInclusions,
            globalTestExclusions:
            globalTestExclusions);
        var options = AnalysisScaffolding.CreateOptions(sonarLintXml);
        var sut = CreateSut(compilation, options);

        sut.ShouldAnalyzeFile(filePath).Should().Be(shouldAnalyze);
    }
}
