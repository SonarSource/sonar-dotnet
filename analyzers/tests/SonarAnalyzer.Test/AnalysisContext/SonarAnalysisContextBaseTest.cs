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

using SonarAnalyzer.Core.AnalysisContext;
using SonarAnalyzer.CSharp.Core.Syntax.Utilities;

namespace SonarAnalyzer.Test.AnalysisContext;

[TestClass]
public partial class SonarAnalysisContextBaseTest
{
    private const string MainTag = "MainSourceScope";
    private const string TestTag = "TestSourceScope";
    private const string UtilityTag = "Utility";
    private const string DummyID = "Sxxx";
    private const string StyleID = "Txxx";

    public TestContext TestContext { get; set; }

    [TestMethod]
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
        var sut = CreateSut(projectType, false);
        sut.HasMatchingScope(AnalysisScaffolding.CreateDescriptor(DummyID, ruleTags)).Should().Be(expectedResult);
        sut.HasMatchingScope(AnalysisScaffolding.CreateDescriptor(StyleID, ruleTags)).Should().Be(expectedResult);
    }

    [TestMethod]
    [DataRow(true, DummyID, ProjectType.Product, MainTag)]
    [DataRow(true, StyleID, ProjectType.Product, MainTag)]
    [DataRow(true, DummyID, ProjectType.Product, MainTag, UtilityTag)]
    [DataRow(true, DummyID, ProjectType.Product, MainTag, TestTag)]
    [DataRow(true, StyleID, ProjectType.Product, MainTag, TestTag)]
    [DataRow(true, DummyID, ProjectType.Product, MainTag, TestTag, UtilityTag)]
    [DataRow(true, DummyID, ProjectType.Test, TestTag)]
    [DataRow(true, StyleID, ProjectType.Test, TestTag)]
    [DataRow(true, DummyID, ProjectType.Test, TestTag, UtilityTag)]
    [DataRow(true, DummyID, ProjectType.Test, MainTag, TestTag, UtilityTag)]    // Utility rules with scope Test&Main do run on test code under scanner context.
    [DataRow(false, DummyID, ProjectType.Test, MainTag, TestTag)]               // Rules with scope Test&Main do not run on test code under scanner context for now.
    [DataRow(true, StyleID, ProjectType.Test, MainTag, TestTag)]                // Style rules with Test&Main scope do run on test code under scanner context
    [DataRow(false, DummyID, ProjectType.Product, TestTag)]
    [DataRow(false, StyleID, ProjectType.Product, TestTag)]
    [DataRow(false, DummyID, ProjectType.Product, TestTag, UtilityTag)]
    [DataRow(false, DummyID, ProjectType.Product, TestTag, TestTag)]
    [DataRow(false, StyleID, ProjectType.Product, TestTag, TestTag)]
    [DataRow(false, DummyID, ProjectType.Test, MainTag)]
    [DataRow(false, StyleID, ProjectType.Test, MainTag)]
    [DataRow(false, DummyID, ProjectType.Test, MainTag, UtilityTag)]
    [DataRow(false, DummyID, ProjectType.Test, MainTag, MainTag)]
    [DataRow(false, StyleID, ProjectType.Test, MainTag, MainTag)]
    public void HasMatchingScope_SingleDiagnostic_WithOneOrMoreScopes_Scanner(bool expectedResult, string id, ProjectType projectType, params string[] ruleTags)
    {
        var diagnostic = AnalysisScaffolding.CreateDescriptor(id, ruleTags);
        CreateSut(projectType, true).HasMatchingScope(diagnostic).Should().Be(expectedResult);
    }

    [TestMethod]
    [DataRow(true, ProjectType.Product, MainTag, MainTag)]
    [DataRow(true, ProjectType.Product, MainTag, MainTag)]
    [DataRow(true, ProjectType.Product, MainTag, TestTag)]
    [DataRow(true, ProjectType.Test, TestTag, TestTag)]
    [DataRow(true, ProjectType.Test, TestTag, MainTag)]
    [DataRow(false, ProjectType.Product, TestTag, TestTag)]
    [DataRow(false, ProjectType.Test, MainTag, MainTag)]
    public void HasMatchingScope_MultipleDiagnostics_WithSingleScope_SonarLint(bool expectedResult, ProjectType projectType, params string[] rulesTag)
    {
        var diagnostics = rulesTag.Select(x => AnalysisScaffolding.CreateDescriptor(DummyID, x)).ToImmutableArray();
        CreateSut(projectType, false).HasMatchingScope(diagnostics).Should().Be(expectedResult);
    }

    [TestMethod]
    [DataRow(true, ProjectType.Product, MainTag, MainTag)]
    [DataRow(true, ProjectType.Product, MainTag, TestTag)]
    [DataRow(true, ProjectType.Test, TestTag, TestTag)]
    [DataRow(true, ProjectType.Test, TestTag, MainTag)]    // Rules with scope Test&Main will run to let the Test diagnostics to be detected. ReportDiagnostic should filter Main issues out.
    [DataRow(false, ProjectType.Product, TestTag, TestTag)]
    [DataRow(false, ProjectType.Test, MainTag, MainTag)]
    public void HasMatchingScope_MultipleDiagnostics_WithSingleScope_Scanner(bool expectedResult, ProjectType projectType, params string[] rulesTag)
    {
        var diagnostics = rulesTag.Select(x => AnalysisScaffolding.CreateDescriptor(DummyID, x)).ToImmutableArray();
        CreateSut(projectType, true).HasMatchingScope(diagnostics).Should().Be(expectedResult);
    }

    [TestMethod]
    public void ProjectConfiguration_LoadsExpectedValues()
    {
        var options = AnalysisScaffolding.CreateOptions(@"TestResources\SonarProjectConfig\Path_Windows\SonarProjectConfig.xml");
        var config = CreateSut(options).ProjectConfiguration();

        config.AnalysisConfigPath.Should().Be(@"c:\foo\bar\.sonarqube\conf\SonarQubeAnalysisConfig.xml");
    }

    [TestMethod]
    public void ProjectConfiguration_UsesCachedValue()
    {
        var options = AnalysisScaffolding.CreateOptions(@"TestResources\SonarProjectConfig\Path_Windows\SonarProjectConfig.xml");
        var firstSut = CreateSut(options);
        var secondSut = CreateSut(options);
        var firstConfig = firstSut.ProjectConfiguration();
        var secondConfig = secondSut.ProjectConfiguration();

        secondConfig.Should().BeSameAs(firstConfig);
    }

    [TestMethod]
    public void ProjectConfiguration_WhenFileChanges_RebuildsCache()
    {
        var firstOptions = AnalysisScaffolding.CreateOptions(@"TestResources\SonarProjectConfig\Path_Windows\SonarProjectConfig.xml");
        var secondOptions = AnalysisScaffolding.CreateOptions(@"TestResources\SonarProjectConfig\Path_Unix\SonarProjectConfig.xml");
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
        var sut = CreateSut(AnalysisScaffolding.CreateOptions(@"ThisPathDoesNotExist\SonarProjectConfig.xml"));

        sut.Invoking(x => x.ProjectConfiguration())
           .Should()
           .Throw<InvalidOperationException>()
           .WithMessage("File 'SonarProjectConfig.xml' has been added as an AdditionalFile but could not be read and parsed.");
    }

    [TestMethod]
    public void ProjectConfiguration_WhenInvalidXml_ThrowException()
    {
        var sut = CreateSut(AnalysisScaffolding.CreateOptions(@"TestResources\SonarProjectConfig\Invalid_Xml\SonarProjectConfig.xml"));

        sut.Invoking(x => x.ProjectConfiguration())
           .Should()
           .Throw<InvalidOperationException>()
           .WithMessage("File 'SonarProjectConfig.xml' has been added as an AdditionalFile but could not be read and parsed.");
    }

    [TestMethod]
    [DataRow("cs")]
    [DataRow("vbnet")]
    public void SonarLintFile_LoadsExpectedValues(string language)
    {
        var analyzerLanguage = language == "cs" ? AnalyzerLanguage.CSharp : AnalyzerLanguage.VisualBasic;
        var (compilation, _) = CreateDummyCompilation(analyzerLanguage, GeneratedFileName);
        var options = AnalysisScaffolding.CreateOptions($@"TestResources\SonarLintXml\All_properties_{language}\SonarLint.xml");
        var sut = CreateSut(compilation, options).SonarLintXml();

        sut.IgnoreHeaderComments(analyzerLanguage.LanguageName).Should().BeTrue();
        sut.AnalyzeGeneratedCode(analyzerLanguage.LanguageName).Should().BeFalse();
        sut.AnalyzeRazorCode(analyzerLanguage.LanguageName).Should().BeFalse();
        AssertArrayContent(sut.Exclusions, nameof(sut.Exclusions));
        AssertArrayContent(sut.Inclusions, nameof(sut.Inclusions));
        AssertArrayContent(sut.GlobalExclusions, nameof(sut.GlobalExclusions));
        AssertArrayContent(sut.TestExclusions, nameof(sut.TestExclusions));
        AssertArrayContent(sut.TestInclusions, nameof(sut.TestInclusions));
        AssertArrayContent(sut.GlobalTestExclusions, nameof(sut.GlobalTestExclusions));

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
        var options = AnalysisScaffolding.CreateOptions(@"TestResources\SonarLintXml\All_properties_cs\SonarLint.xml");
        var firstSut = CreateSut(options);
        var secondSut = CreateSut(options);
        var firstFile = firstSut.SonarLintXml();
        var secondFile = secondSut.SonarLintXml();

        secondFile.Should().BeSameAs(firstFile);
    }

    [TestMethod]
    public void SonarLintFile_WhenFileChanges_RebuildsCache()
    {
        var firstOptions = AnalysisScaffolding.CreateOptions(@"TestResources\SonarLintXml\All_properties_cs\SonarLint.xml");
        var secondOptions = AnalysisScaffolding.CreateOptions(@"TestResources\SonarLintXml\All_properties_vbnet\SonarLint.xml");
        var firstFile = CreateSut(firstOptions).SonarLintXml();
        var secondFile = CreateSut(secondOptions).SonarLintXml();

        secondFile.Should().NotBeSameAs(firstFile);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow(@"\foo\bar\does-not-exit")]
    [DataRow(@"\foo\bar\x.xml")]
    [DataRow("path//aSonarLint.xml")] // different name
    [DataRow("path//SonarLint.xmla")] // different extension
    public void SonarLintFile_WhenAdditionalFileNotPresent_ReturnsDefaultValues(string folder)
    {
        var sut = CreateSut(AnalysisScaffolding.CreateOptions(folder)).SonarLintXml();
        CheckSonarLintXmlDefaultValues(sut);
    }

    [TestMethod]
    public void SonarLintFile_WhenInvalidXml_ReturnsDefaultValues()
    {
        var sut = CreateSut(AnalysisScaffolding.CreateOptions(@"TestResources\SonarLintXml\Invalid_Xml\SonarLint.xml")).SonarLintXml();
        CheckSonarLintXmlDefaultValues(sut);
    }

    [TestMethod]
    public void SonarLintFile_WhenFileIsMissing_ThrowException()
    {
        var sut = CreateSut(AnalysisScaffolding.CreateOptions(@"ThisPathDoesNotExist\SonarLint.xml"));

        sut.Invoking(x => x.SonarLintXml())
           .Should()
           .Throw<InvalidOperationException>()
           .WithMessage("File 'SonarLint.xml' has been added as an AdditionalFile but could not be read and parsed.");
    }

    [TestMethod]
    public void ReportIssue_Null_Throws()
    {
        var compilation = TestCompiler.CompileCS("// Nothing to see here").Model.Compilation;
        var sut = CreateSut(ProjectType.Product, false);
        var rule = AnalysisScaffolding.CreateDescriptor("Sxxxx", DiagnosticDescriptorFactory.MainSourceScopeTag);
        var recognizer = CSharpGeneratedCodeRecognizer.Instance;

        sut.Invoking(x => x.ReportIssue(recognizer, null, primaryLocation: null, secondaryLocations: [])).Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("rule");
        sut.Invoking(x => x.ReportIssue(recognizer, rule, primaryLocation: null, secondaryLocations: null)).Should().NotThrow();
    }

    [TestMethod]
    public void ReportIssue_NullLocation_UsesEmpty()
    {
        Diagnostic lastDiagnostic = null;
        var compilation = TestCompiler.CompileCS("// Nothing to see here").Model.Compilation;
        var compilationContext = new CompilationAnalysisContext(compilation, AnalysisScaffolding.CreateOptions(), x => lastDiagnostic = x, _ => true, default);
        var sut = new SonarCompilationReportingContext(AnalysisScaffolding.CreateSonarAnalysisContext(), compilationContext);
        var rule = AnalysisScaffolding.CreateDescriptor("Sxxxx", DiagnosticDescriptorFactory.MainSourceScopeTag);
        var recognizer = CSharpGeneratedCodeRecognizer.Instance;

        sut.ReportIssue(recognizer, rule, location: null);
        lastDiagnostic.Should().NotBeNull();
        lastDiagnostic.Location.Should().Be(Location.None);

        sut.ReportIssue(recognizer, rule, primaryLocation: null, secondaryLocations: []);
        lastDiagnostic.Should().NotBeNull();
        lastDiagnostic.Location.Should().Be(Location.None);
    }

    private static void CheckSonarLintXmlDefaultValues(SonarLintXmlReader sut)
    {
        sut.AnalyzeRazorCode(LanguageNames.CSharp).Should().BeTrue();
        sut.AnalyzeGeneratedCode(LanguageNames.CSharp).Should().BeFalse();
        sut.IgnoreHeaderComments(LanguageNames.CSharp).Should().BeFalse();
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
