/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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

using System.IO;
using SonarAnalyzer.AnalysisContext;
using SonarAnalyzer.CFG.Helpers;
using SonarAnalyzer.Rules;
using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class AnalysisWarningAnalyzerTest
{
    public TestContext TestContext { get; set; }

    [DataTestMethod]
    [DataRow(LanguageNames.CSharp, true)]
    [DataRow(LanguageNames.CSharp, false)]
    [DataRow(LanguageNames.VisualBasic, true)]
    [DataRow(LanguageNames.VisualBasic, false)]
    public void AnalysisWarning_MSBuildSupportedScenario_NoWarning(string languageName, bool isAnalyzerEnabled)
    {
        var expectedPath = ExecuteAnalyzer(languageName, isAnalyzerEnabled, RoslynHelper.VS2017MajorVersion, RoslynHelper.MinimalSupportedMajorVersion); // Using production value that is lower than our UT Roslyn version
        File.Exists(expectedPath).Should().BeFalse("Analysis warning file should not be generated.");
    }

    [DataTestMethod]
    [DataRow(LanguageNames.CSharp)]
    [DataRow(LanguageNames.VisualBasic)]
    public void AnalysisWarning_MSBuild14UnsupportedScenario_GenerateWarning(string languageName)
    {
        var expectedPath = ExecuteAnalyzer(languageName, true, 1000, 1001); // Requiring too high Roslyn version => we're under unsupported scenario
        File.Exists(expectedPath).Should().BeTrue();
        File.ReadAllText(expectedPath).Should().Be("""[{"text": "The analysis using MsBuild 14 is no longer supported and the analysis with MsBuild 15 is deprecated. Please update your pipeline to MsBuild 16 or higher."}]""");
    }

    [DataTestMethod]
    [DataRow(LanguageNames.CSharp)]
    [DataRow(LanguageNames.VisualBasic)]
    public void AnalysisWarning_MSBuild15DeprecatedScenario_GenerateWarning(string languageName)
    {
        var expectedPath = ExecuteAnalyzer(languageName, true, RoslynHelper.VS2017MajorVersion, 1000); // Requiring too high Roslyn version => we're under unsupported scenario
        File.Exists(expectedPath).Should().BeTrue();
        File.ReadAllText(expectedPath).Should().Be("""[{"text": "The analysis using MsBuild 15 is deprecated. Please update your pipeline to MsBuild 16 or higher."}]""");
    }

    [DataTestMethod]
    [DataRow(LanguageNames.CSharp)]
    [DataRow(LanguageNames.VisualBasic)]
    public void AnalysisWarning_LockFile_PathShouldBeReused(string languageName)
    {
        var expectedPath = ExecuteAnalyzer(languageName, true, RoslynHelper.VS2017MajorVersion, 1000);
        // Lock file and run it for 2nd time
        using var lockedFile = new FileStream(expectedPath, FileMode.Open, FileAccess.Write, FileShare.None);
        ExecuteAnalyzer(languageName, true, RoslynHelper.VS2017MajorVersion, 1000).Should().Be(expectedPath, "path should be reused and analyzer should not fail");
    }

    [DataTestMethod]
    [DataRow(LanguageNames.CSharp)]
    [DataRow(LanguageNames.VisualBasic)]
    public void AnalysisWarning_FileExceptions_AreIgnored(string languageName)
    {
        // This will not create the output directory, causing an exception in the File.WriteAllText(...)
        var expectedPath = ExecuteAnalyzer(languageName, true, 500, 1000, false);  // Requiring too high Roslyn version => we're under unsupported scenario
        File.Exists(expectedPath).Should().BeFalse();
    }

    private string ExecuteAnalyzer(string languageName, bool isAnalyzerEnabled, int vs2017MajorVersion, int minimalSupportedRoslynVersion, bool createDirectory = true)
    {
        var language = AnalyzerLanguage.FromName(languageName);
        var analysisOutPath = TestHelper.TestPath(TestContext, @$"{languageName}\.sonarqube\out");
        var projectOutPath = Path.GetFullPath(Path.Combine(analysisOutPath, "0", "output-language"));
        if (createDirectory)
        {
            Directory.CreateDirectory(analysisOutPath);
        }
        UtilityAnalyzerBase analyzer = language.LanguageName switch
        {
            LanguageNames.CSharp => new TestAnalysisWarningAnalyzer_CS(isAnalyzerEnabled, vs2017MajorVersion, minimalSupportedRoslynVersion, projectOutPath),
            LanguageNames.VisualBasic => new TestAnalysisWarningAnalyzer_VB(isAnalyzerEnabled, vs2017MajorVersion, minimalSupportedRoslynVersion, projectOutPath),
            _ => throw new UnexpectedLanguageException(language)
        };
        new VerifierBuilder().AddAnalyzer(() => analyzer).AddSnippet(string.Empty).VerifyNoIssues(); // Nothing to analyze, just make it run
        return Path.Combine(analysisOutPath, "AnalysisWarnings.MsBuild.json");
    }

    private sealed class TestAnalysisWarningAnalyzer_CS : CS.AnalysisWarningAnalyzer
    {
        private readonly bool isAnalyzerEnabled;
        private readonly string outPath;
        protected override int VS2017MajorVersion { get; }
        protected override int MinimalSupportedRoslynVersion { get; }

        public TestAnalysisWarningAnalyzer_CS(bool isAnalyzerEnabled, int vs2017MajorVersion, int minimalSupportedRoslynVersion, string outPath)
        {
            this.isAnalyzerEnabled = isAnalyzerEnabled;
            VS2017MajorVersion = vs2017MajorVersion;
            MinimalSupportedRoslynVersion = minimalSupportedRoslynVersion;
            this.outPath = outPath;
        }

        protected override UtilityAnalyzerParameters ReadParameters<T>(SonarAnalysisContextBase<T> context) =>
            base.ReadParameters(context) with { IsAnalyzerEnabled = isAnalyzerEnabled, OutPath = outPath };
    }

    private sealed class TestAnalysisWarningAnalyzer_VB : VB.AnalysisWarningAnalyzer
    {
        private readonly bool isAnalyzerEnabled;
        private readonly string outPath;
        protected override int VS2017MajorVersion { get; }
        protected override int MinimalSupportedRoslynVersion { get; }

        public TestAnalysisWarningAnalyzer_VB(bool isAnalyzerEnabled, int vs2017MajorVersion, int minimalSupportedRoslynVersion, string outPath)
        {
            this.isAnalyzerEnabled = isAnalyzerEnabled;
            VS2017MajorVersion = vs2017MajorVersion;
            MinimalSupportedRoslynVersion = minimalSupportedRoslynVersion;
            this.outPath = outPath;
        }

        protected override UtilityAnalyzerParameters ReadParameters<T>(SonarAnalysisContextBase<T> context) =>
            base.ReadParameters(context) with { IsAnalyzerEnabled = isAnalyzerEnabled, OutPath = outPath };
    }
}
