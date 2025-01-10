/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.VisualBasic;
using SonarAnalyzer.AnalysisContext;
using SonarAnalyzer.Rules;

namespace SonarAnalyzer.Test.Rules.Utilities
{
    [TestClass]
    public class UtilityAnalyzerBaseTest
    {
        private const string DefaultSonarProjectConfig = @"TestResources\SonarProjectConfig\Path_Windows\SonarProjectConfig.xml";
        private const string DefaultProjectOutFolderPath = @"TestResources\ProjectOutFolderPath.txt";
        public TestContext TestContext { get; set; }

        [DataTestMethod]
        [DataRow(LanguageNames.CSharp, DefaultProjectOutFolderPath, @"path\output-cs")]
        [DataRow(LanguageNames.VisualBasic, DefaultProjectOutFolderPath, @"path\output-vbnet")]
        [DataRow(LanguageNames.CSharp, DefaultSonarProjectConfig, @"C:\foo\bar\.sonarqube\out\0\output-cs")]
        [DataRow(LanguageNames.VisualBasic, DefaultSonarProjectConfig, @"C:\foo\bar\.sonarqube\out\0\output-vbnet")]
        public void ReadConfig_OutPath(string language, string additionalPath, string expectedOutPath)
        {
            // We do not test what is read from the SonarLint file, but we need it
            var utilityAnalyzer = new TestUtilityAnalyzer(language, @"TestResources\SonarLintXml\All_properties_cs\SonarLint.xml", additionalPath);

            utilityAnalyzer.Parameters.OutPath.Should().Be(expectedOutPath);
            utilityAnalyzer.Parameters.IsAnalyzerEnabled.Should().BeTrue();
        }

        [DataTestMethod]
        [DataRow(DefaultProjectOutFolderPath, DefaultSonarProjectConfig)]
        [DataRow(DefaultSonarProjectConfig, DefaultProjectOutFolderPath)]
        public void ReadConfig_OutPath_FromSonarProjectConfig_HasPriority(string firstFile, string secondFile)
        {
            // We do not test what is read from the SonarLint file, but we need it
            var utilityAnalyzer = new TestUtilityAnalyzer(LanguageNames.CSharp, @"TestResources\SonarLintXml\All_properties_cs\SonarLint.xml", firstFile, secondFile);

            utilityAnalyzer.Parameters.OutPath.Should().Be(@"C:\foo\bar\.sonarqube\out\0\output-cs");
            utilityAnalyzer.Parameters.IsAnalyzerEnabled.Should().BeTrue();
        }

        [DataTestMethod]
        [DataRow(LanguageNames.CSharp, true)]
        [DataRow(LanguageNames.CSharp, false)]
        [DataRow(LanguageNames.VisualBasic, true)]
        [DataRow(LanguageNames.VisualBasic, false)]
        public void ReadsSettings_AnalyzeGenerated(string language, bool analyzeGenerated)
        {
            var sonarLintXmlPath = AnalysisScaffolding.CreateSonarLintXml(TestContext, language: language, analyzeGeneratedCode: analyzeGenerated);
            var utilityAnalyzer = new TestUtilityAnalyzer(language, sonarLintXmlPath, DefaultSonarProjectConfig);

            utilityAnalyzer.Parameters.AnalyzeGeneratedCode.Should().Be(analyzeGenerated);
            utilityAnalyzer.Parameters.IsAnalyzerEnabled.Should().BeTrue();
        }

        [DataTestMethod]
        [DataRow(LanguageNames.CSharp, true)]
        [DataRow(LanguageNames.CSharp, false)]
        [DataRow(LanguageNames.VisualBasic, true)]
        [DataRow(LanguageNames.VisualBasic, false)]
        public void ReadsSettings_IgnoreHeaderComments(string language, bool ignoreHeaderComments)
        {
            var sonarLintXmlPath = AnalysisScaffolding.CreateSonarLintXml(TestContext, language: language, ignoreHeaderComments: ignoreHeaderComments);
            var utilityAnalyzer = new TestUtilityAnalyzer(language, sonarLintXmlPath, DefaultSonarProjectConfig);

            utilityAnalyzer.Parameters.IgnoreHeaderComments.Should().Be(ignoreHeaderComments);
            utilityAnalyzer.Parameters.IsAnalyzerEnabled.Should().BeTrue();
        }

        [TestMethod]
        public void NoSonarLintXml_AnalyzerNotEnabled()
        {
            new TestUtilityAnalyzer(LanguageNames.CSharp, DefaultProjectOutFolderPath).Parameters.IsAnalyzerEnabled.Should().BeFalse();
            new TestUtilityAnalyzer(LanguageNames.CSharp, DefaultSonarProjectConfig).Parameters.IsAnalyzerEnabled.Should().BeFalse();
        }

        [TestMethod]
        public void NoOutputPath_AnalyzerNotEnabled() =>
            new TestUtilityAnalyzer(LanguageNames.CSharp, AnalysisScaffolding.CreateSonarLintXml(TestContext, analyzeGeneratedCode: true)).Parameters.IsAnalyzerEnabled.Should().BeFalse();

        [TestMethod]
        public void GetTextRange()
        {
            var fileLinePositionSpan = new FileLinePositionSpan(
                "path",
                new LinePosition(55, 42),
                new LinePosition(99, 9313));

            // Act
            var result = UtilityAnalyzerBase.GetTextRange(fileLinePositionSpan);

            // Assert
            result.Should().NotBeNull();
            // line number in SQ starts from 1, Roslyn starts from 0
            result.StartLine.Should().Be(55 + 1);
            result.StartOffset.Should().Be(42);
            result.EndLine.Should().Be(99 + 1);
            result.EndOffset.Should().Be(9313);
        }

        [DiagnosticAnalyzer(LanguageNames.CSharp)]
        private class TestUtilityAnalyzer : UtilityAnalyzerBase
        {
            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => throw new NotImplementedException();
            public UtilityAnalyzerParameters Parameters { get; }

            public TestUtilityAnalyzer(string language, params string[] additionalPaths) : base("S9999-test", "Title")
            {
                var additionalFiles = additionalPaths.Select(x => new AnalyzerAdditionalFile(x)).ToImmutableArray<AdditionalText>();
                Compilation compilation = language switch
                {
                    LanguageNames.CSharp => CSharpCompilation.Create(null),
                    LanguageNames.VisualBasic => VisualBasicCompilation.Create(null),
                    _ => throw new InvalidOperationException($"Unexpected {nameof(language)}: {language}")
                };
                var context = new CompilationAnalysisContext(compilation, new AnalyzerOptions(additionalFiles), null, null, default);
                Parameters = ReadParameters(new SonarCompilationReportingContext(AnalysisScaffolding.CreateSonarAnalysisContext(), context));
            }

            protected override void Initialize(SonarAnalysisContext context) =>
                throw new NotSupportedException();
        }
    }
}
