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

using SonarAnalyzer.AnalysisContext;
using SonarAnalyzer.Common;

namespace SonarAnalyzer.UnitTest.AnalysisContext;

[TestClass]
public partial class SonarAnalysisContextBaseTest
{
    private const string MainTag = "MainSourceScope";
    private const string TestTag = "TestSourceScope";
    private const string UtilityTag = "Utility";
    private const string DummyID = "Sxxx";

    public TestContext TestContext { get; set; }

    [DataTestMethod]
    [DataRow(true, ProjectType.Product, MainTag)]
    [DataRow(true, ProjectType.Product, MainTag, UtilityTag)]
    [DataRow(true, ProjectType.Product, MainTag, TestTag)]
    [DataRow(true, ProjectType.Product, MainTag, TestTag, UtilityTag)]
    [DataRow(true, ProjectType.Test, TestTag)]
    [DataRow(true, ProjectType.Test, TestTag, UtilityTag)]
    [DataRow(true, ProjectType.Test, MainTag, TestTag)]
    [DataRow(true, ProjectType.Test, MainTag, TestTag, UtilityTag)]
    [DataRow(false, ProjectType.Product, TestTag)]
    [DataRow(false, ProjectType.Product, TestTag, TestTag)]
    [DataRow(false, ProjectType.Test, MainTag)]
    [DataRow(false, ProjectType.Test, MainTag, MainTag)]
    public void HasMatchingScope_SingleDiagnostic_WithOneOrMoreScopes_SonarLint(bool expectedResult, ProjectType projectType, params string[] ruleTags)
    {
        var diagnostic = AnalysisScaffolding.CreateDescriptor(DummyID, ruleTags);
        CreateSut(projectType, false).HasMatchingScope(diagnostic).Should().Be(expectedResult);
    }

    [DataTestMethod]
    [DataRow(true, ProjectType.Product, MainTag)]
    [DataRow(true, ProjectType.Product, MainTag, UtilityTag)]
    [DataRow(true, ProjectType.Product, MainTag, TestTag)]
    [DataRow(true, ProjectType.Product, MainTag, TestTag, UtilityTag)]
    [DataRow(true, ProjectType.Test, TestTag)]
    [DataRow(true, ProjectType.Test, TestTag, UtilityTag)]
    [DataRow(true, ProjectType.Test, MainTag, TestTag, UtilityTag)]     // Utility rules with scope Test&Main do run on test code under scanner context.
    [DataRow(false, ProjectType.Test, MainTag, TestTag)]                // Rules with scope Test&Main do not run on test code under scanner context for now.
    [DataRow(false, ProjectType.Product, TestTag)]
    [DataRow(false, ProjectType.Product, TestTag, UtilityTag)]
    [DataRow(false, ProjectType.Product, TestTag, TestTag)]
    [DataRow(false, ProjectType.Test, MainTag)]
    [DataRow(false, ProjectType.Test, MainTag, UtilityTag)]
    [DataRow(false, ProjectType.Test, MainTag, MainTag)]
    public void HasMatchingScope_SingleDiagnostic_WithOneOrMoreScopes_Scanner(bool expectedResult, ProjectType projectType, params string[] ruleTags)
    {
        var diagnostic = AnalysisScaffolding.CreateDescriptor(DummyID, ruleTags);
        CreateSut(projectType, true).HasMatchingScope(diagnostic).Should().Be(expectedResult);
    }

    [DataTestMethod]
    [DataRow(true, ProjectType.Product, MainTag, MainTag)]
    [DataRow(true, ProjectType.Product, MainTag, MainTag)]
    [DataRow(true, ProjectType.Product, MainTag, TestTag)]
    [DataRow(true, ProjectType.Test, TestTag, TestTag)]
    [DataRow(true, ProjectType.Test, TestTag, MainTag)]
    [DataRow(false, ProjectType.Product, TestTag, TestTag)]
    [DataRow(false, ProjectType.Test, MainTag, MainTag)]
    public void HasMatchingScope_MultipleDiagnostics_WithSingleScope_SonarLint(bool expectedResult, ProjectType projectType, params string[] rulesTag)
    {
        var diagnostics = rulesTag.Select(x => AnalysisScaffolding.CreateDescriptor(DummyID, x));
        CreateSut(projectType, false).HasMatchingScope(diagnostics).Should().Be(expectedResult);
    }

    [DataTestMethod]
    [DataRow(true, ProjectType.Product, MainTag, MainTag)]
    [DataRow(true, ProjectType.Product, MainTag, TestTag)]
    [DataRow(true, ProjectType.Test, TestTag, TestTag)]
    [DataRow(true, ProjectType.Test, TestTag, MainTag)]    // Rules with scope Test&Main will run to let the Test diagnostics to be detected. ReportDiagnostic should filter Main issues out.
    [DataRow(false, ProjectType.Product, TestTag, TestTag)]
    [DataRow(false, ProjectType.Test, MainTag, MainTag)]
    public void HasMatchingScope_MultipleDiagnostics_WithSingleScope_Scanner(bool expectedResult, ProjectType projectType, params string[] rulesTag)
    {
        var diagnostics = rulesTag.Select(x => AnalysisScaffolding.CreateDescriptor(DummyID, x));
        CreateSut(projectType, true).HasMatchingScope(diagnostics).Should().Be(expectedResult);
    }

    [TestMethod]
    public void ProjectConfiguration_LoadsExpectedValues()
    {
        var options = AnalysisScaffolding.CreateOptions($@"ResourceTests\SonarProjectConfig\Path_Windows\SonarProjectConfig.xml");
        var config = CreateSut(options).ProjectConfiguration();

        config.AnalysisConfigPath.Should().Be(@"c:\foo\bar\.sonarqube\conf\SonarQubeAnalysisConfig.xml");
    }

    [TestMethod]
    public void ProjectConfiguration_UsesCachedValue()
    {
        var options = AnalysisScaffolding.CreateOptions($@"ResourceTests\SonarProjectConfig\Path_Windows\SonarProjectConfig.xml");
        var firstSut = CreateSut(options);
        var secondSut = CreateSut(options);
        var firstConfig = firstSut.ProjectConfiguration();
        var secondConfig = secondSut.ProjectConfiguration();

        secondConfig.Should().BeSameAs(firstConfig);
    }

    [TestMethod]
    public void ProjectConfiguration_WhenFileChanges_RebuildsCache()
    {
        var firstOptions = AnalysisScaffolding.CreateOptions($@"ResourceTests\SonarProjectConfig\Path_Windows\SonarProjectConfig.xml");
        var secondOptions = AnalysisScaffolding.CreateOptions($@"ResourceTests\SonarProjectConfig\Path_Unix\SonarProjectConfig.xml");
        var firstConfig = CreateSut(firstOptions).ProjectConfiguration();
        var secondConfig = CreateSut(secondOptions).ProjectConfiguration();

        secondConfig.Should().NotBeSameAs(firstConfig);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("/foo/bar/does-not-exit")]
    [DataRow("/foo/bar/x.xml")]
    public void ProjectConfiguration_WhenAdditionalFileNotPresent_ReturnsEmptyConfig(string folder)
    {
        var options = AnalysisScaffolding.CreateOptions(folder);
        var config = CreateSut(options).ProjectConfiguration();

        config.AnalysisConfigPath.Should().BeNull();
        config.ProjectPath.Should().BeNull();
        config.FilesToAnalyzePath.Should().BeNull();
        config.OutPath.Should().BeNull();
        config.ProjectType.Should().Be(ProjectType.Unknown);
        config.TargetFramework.Should().BeNull();
    }

    [TestMethod]
    public void ProjectConfiguration_WhenFileIsMissing_ThrowException()
    {
        var sut = CreateSut(AnalysisScaffolding.CreateOptions("ThisPathDoesNotExist\\SonarProjectConfig.xml"));

        sut.Invoking(x => x.ProjectConfiguration())
           .Should()
           .Throw<InvalidOperationException>()
           .WithMessage("File 'SonarProjectConfig.xml' has been added as an AdditionalFile but could not be read and parsed.");
    }

    [TestMethod]
    public void ProjectConfiguration_WhenInvalidXml_ThrowException()
    {
        var sut = CreateSut(AnalysisScaffolding.CreateOptions($@"ResourceTests\SonarProjectConfig\Invalid_Xml\SonarProjectConfig.xml"));

        sut.Invoking(x => x.ProjectConfiguration())
           .Should()
           .Throw<InvalidOperationException>()
           .WithMessage("File 'SonarProjectConfig.xml' has been added as an AdditionalFile but could not be read and parsed.");
    }

    [TestMethod]
    public void SonarLintFile_LoadsExpectedValues()
    {
        var (compilationCS, _) = CreateDummyCompilation(AnalyzerLanguage.CSharp, "ExtraEmptyFile");
        var (compilationVB, _) = CreateDummyCompilation(AnalyzerLanguage.VisualBasic, "OtherFile");
        var optionsCS = AnalysisScaffolding.CreateOptions("ResourceTests\\SonarLintXml\\All_properties_cs\\SonarLint.xml");
        var optionsVB = AnalysisScaffolding.CreateOptions("ResourceTests\\SonarLintXml\\All_properties_vbnet\\SonarLint.xml");
        var sutCS = CreateSut(compilationCS, optionsCS).SonarLintFile();
        var sutVB = CreateSut(compilationVB, optionsVB).SonarLintFile();

        AssertReader(sutCS);
        AssertReader(sutVB);

        static void AssertReader(SonarLintXmlReader sut)
        {
            sut.IgnoreHeaderComments.Should().BeTrue();
            sut.AnalyzeGeneratedCode.Should().BeFalse();
            AssertArrayContent(sut.Exclusions, nameof(sut.Exclusions));
            AssertArrayContent(sut.Inclusions, nameof(sut.Inclusions));
            AssertArrayContent(sut.GlobalExclusions, nameof(sut.GlobalExclusions));
            AssertArrayContent(sut.TestExclusions, nameof(sut.TestExclusions));
            AssertArrayContent(sut.TestInclusions, nameof(sut.TestInclusions));
            AssertArrayContent(sut.GlobalTestExclusions, nameof(sut.GlobalTestExclusions));
        }

        static void AssertArrayContent(string[] array, string folder)
        {
            array.Should().HaveCount(2);
            array[0].Should().BeEquivalentTo($"Fake/{folder}/**/*");
            array[1].Should().BeEquivalentTo($"Fake/{folder}/Second*/**/*");
        }
    }

    [TestMethod]
    public void SonarLintFile_UsesCachedValue()
    {
        var options = AnalysisScaffolding.CreateOptions("ResourceTests\\SonarLintXml\\All_properties_cs\\SonarLint.xml");
        var firstSut = CreateSut(options);
        var secondSut = CreateSut(options);
        var firstFile = firstSut.SonarLintFile();
        var secondFile = secondSut.SonarLintFile();

        secondFile.Should().BeSameAs(firstFile);
    }

    [TestMethod]
    public void SonarLintFile_WhenFileChanges_RebuildsCache()
    {
        var firstOptions = AnalysisScaffolding.CreateOptions("ResourceTests\\SonarLintXml\\All_properties_cs\\SonarLint.xml");
        var secondOptions = AnalysisScaffolding.CreateOptions("ResourceTests\\SonarLintXml\\All_properties_vbnet\\SonarLint.xml");
        var firstFile = CreateSut(firstOptions).SonarLintFile();
        var secondFile = CreateSut(secondOptions).SonarLintFile();

        secondFile.Should().NotBeSameAs(firstFile);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("\\foo\\bar\\does-not-exit")]
    [DataRow("\\foo\\bar\\x.xml")]
    public void SonarLintFile_WhenAdditionalFileNotPresent_ReturnsDefaultValues(string folder)
    {
        var sut = CreateSut(AnalysisScaffolding.CreateOptions(folder)).SonarLintFile();
        CheckSonarLintXmlDefaultValues(sut);
    }

    [TestMethod]
    public void SonarLintFile_WhenInvalidXml_ReturnsDefaultValues()
    {
        var sut = CreateSut(AnalysisScaffolding.CreateOptions("ResourceTests\\SonarLintXml\\Invalid_Xml\\SonarLint.xml")).SonarLintFile();
        CheckSonarLintXmlDefaultValues(sut);
    }

    [TestMethod]
    public void SonarLintFile_WhenFileIsMissing_ThrowException()
    {
        var sut = CreateSut(AnalysisScaffolding.CreateOptions("ThisPathDoesNotExist\\SonarLint.xml"));

        sut.Invoking(x => x.SonarLintFile())
           .Should()
           .Throw<InvalidOperationException>()
           .WithMessage("File 'SonarLint.xml' has been added as an AdditionalFile but could not be read and parsed.");
    }

    private static void CheckSonarLintXmlDefaultValues(SonarLintXmlReader sut)
    {
        sut.AnalyzeGeneratedCode.Should().BeFalse();
        sut.IgnoreHeaderComments.Should().BeFalse();
        sut.Exclusions.Should().NotBeNull().And.HaveCount(0);
        sut.Inclusions.Should().NotBeNull().And.HaveCount(0);
        sut.GlobalExclusions.Should().NotBeNull().And.HaveCount(0);
        sut.TestExclusions.Should().NotBeNull().And.HaveCount(0);
        sut.TestInclusions.Should().NotBeNull().And.HaveCount(0);
        sut.GlobalTestExclusions.Should().NotBeNull().And.HaveCount(0);
    }

    private SonarCompilationReportingContext CreateSut(ProjectType projectType, bool isScannerRun) =>
        CreateSut(AnalysisScaffolding.CreateOptions(AnalysisScaffolding.CreateSonarProjectConfig(TestContext, projectType, isScannerRun)));

    private static SonarCompilationReportingContext CreateSut(AnalyzerOptions options) =>
        CreateSut(new SnippetCompiler("// Nothing to see here").SemanticModel.Compilation, options);

    private static SonarCompilationReportingContext CreateSut(Compilation compilation, AnalyzerOptions options)
    {
        var compilationContext = new CompilationAnalysisContext(compilation, options, _ => { }, _ => true, default);
        return new(AnalysisScaffolding.CreateSonarAnalysisContext(), compilationContext);
    }
}
