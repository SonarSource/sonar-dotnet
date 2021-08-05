/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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

using System;
using System.Collections.Immutable;
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Rules;
using SonarAnalyzer.UnitTest.Helpers;

namespace SonarAnalyzer.UnitTest.Rules.Utilities
{
    [TestClass]
    public class UtilityAnalyzerBaseTest
    {
        private const string DefaultSonarProjectConfig = @"ResourceTests\SonarProjectConfig\Path_Windows\SonarProjectConfig.xml";
        private const string DefaultProjectOutFolderPath = @"ResourceTests\ProjectOutFolderPath.txt";

        [DataTestMethod]
        [DataRow(LanguageNames.CSharp, DefaultProjectOutFolderPath, @"path\output-cs")]
        [DataRow(LanguageNames.VisualBasic, DefaultProjectOutFolderPath, @"path\output-vbnet")]
        [DataRow(LanguageNames.CSharp, DefaultSonarProjectConfig, @"C:\foo\bar\.sonarqube\out\0\output-cs")]
        [DataRow(LanguageNames.VisualBasic, DefaultSonarProjectConfig, @"C:\foo\bar\.sonarqube\out\0\output-vbnet")]
        public void ReadConfig_OutPath(string language, string additionalPath, string expectedOutPath)
        {
            // We do not test what is read from the SonarLint file, but we need it
            var utilityAnalyzer = new TestUtilityAnalyzer(language, @"ResourceTests\SonarLint.xml", additionalPath);

            utilityAnalyzer.TestOutPath.Should().Be(expectedOutPath);
            utilityAnalyzer.TestIsAnalyzerEnabled.Should().BeTrue();
        }

        [DataTestMethod]
        [DataRow(DefaultProjectOutFolderPath, DefaultSonarProjectConfig)]
        [DataRow(DefaultSonarProjectConfig, DefaultProjectOutFolderPath)]
        public void ReadConfig_OutPath_FromSonarProjectConfig_HasPriority(string firstFile, string secondFile)
        {
            // We do not test what is read from the SonarLint file, but we need it
            var utilityAnalyzer = new TestUtilityAnalyzer(LanguageNames.CSharp, @"ResourceTests\SonarLint.xml", firstFile, secondFile);

            utilityAnalyzer.TestOutPath.Should().Be(@"C:\foo\bar\.sonarqube\out\0\output-cs");
            utilityAnalyzer.TestIsAnalyzerEnabled.Should().BeTrue();
        }

        [DataTestMethod]
        [DataRow(LanguageNames.CSharp, @"ResourceTests\AnalyzeGeneratedTrue\SonarLint.xml", true)]
        [DataRow(LanguageNames.CSharp, @"ResourceTests\AnalyzeGeneratedFalse\SonarLint.xml", false)]
        [DataRow(LanguageNames.VisualBasic, @"ResourceTests\AnalyzeGeneratedTrueVbnet\SonarLint.xml", true)]
        [DataRow(LanguageNames.VisualBasic, @"ResourceTests\AnalyzeGeneratedFalseVbnet\SonarLint.xml", false)]
        public void ReadsSettings_AnalyzeGenerated(string language, string sonarLintXmlPath, bool expectedAnalyzeGeneratedCodeValue)
        {
            var utilityAnalyzer = new TestUtilityAnalyzer(language, sonarLintXmlPath, DefaultSonarProjectConfig);

            utilityAnalyzer.TestAnalyzeGeneratedCode.Should().Be(expectedAnalyzeGeneratedCodeValue);
            utilityAnalyzer.TestIsAnalyzerEnabled.Should().BeTrue();
        }

        [DataTestMethod]
        [DataRow(LanguageNames.CSharp, @"ResourceTests\IgnoreHeaderCommentsTrueCSharp\SonarLint.xml", true)]
        [DataRow(LanguageNames.CSharp, @"ResourceTests\IgnoreHeaderCommentsFalseCSharp\SonarLint.xml", false)]
        [DataRow(LanguageNames.VisualBasic, @"ResourceTests\IgnoreHeaderCommentsTrueVbnet\SonarLint.xml", true)]
        [DataRow(LanguageNames.VisualBasic, @"ResourceTests\IgnoreHeaderCommentsFalseVbnet\SonarLint.xml", false)]
        public void ReadsSettings_IgnoreHeaderComments(string language, string sonarLintXmlPath, bool expectedIgnoreHeaderComments)
        {
            var utilityAnalyzer = new TestUtilityAnalyzer(language, sonarLintXmlPath, DefaultSonarProjectConfig);

            utilityAnalyzer.TestIgnoreHeaderComments.Should().Be(expectedIgnoreHeaderComments);
            utilityAnalyzer.TestIsAnalyzerEnabled.Should().BeTrue();
        }

        [TestMethod]
        public void NoSonarLintXml_AnalyzerNotEnabled()
        {
            new TestUtilityAnalyzer(LanguageNames.CSharp, DefaultProjectOutFolderPath).TestIsAnalyzerEnabled.Should().BeFalse();
            new TestUtilityAnalyzer(LanguageNames.CSharp, DefaultSonarProjectConfig).TestIsAnalyzerEnabled.Should().BeFalse();
        }

        [TestMethod]
        public void NoOutputPath_AnalyzerNotEnabled() =>
            new TestUtilityAnalyzer(LanguageNames.CSharp, @"ResourceTests\AnalyzeGeneratedTrue\SonarLint.xml").TestIsAnalyzerEnabled.Should().BeFalse();

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
            public bool TestIsAnalyzerEnabled => IsAnalyzerEnabled;
            public bool TestAnalyzeGeneratedCode => AnalyzeGeneratedCode;
            public bool TestIgnoreHeaderComments => IgnoreHeaderComments;
            public string TestOutPath => OutPath;

            public TestUtilityAnalyzer(string language, params string[] additionalPaths) : base("S9999-test", "Title")
            {
                var additionalFiles = additionalPaths.Select(x => new AnalyzerAdditionalFile(x)).ToImmutableArray<AdditionalText>();
                var context = new SonarAnalysisContext(new SonarAnalysisContextTest.DummyContext(), Enumerable.Empty<DiagnosticDescriptor>());
                Compilation compilation = language switch
                {
                    LanguageNames.CSharp => CSharpCompilation.Create(null),
                    LanguageNames.VisualBasic => VisualBasicCompilation.Create(null),
                    _ => throw new InvalidOperationException($"Unexpected {nameof(language)}: {language}")
                };
                var c = new CompilationAnalysisContext(compilation, new AnalyzerOptions(additionalFiles), null, null, default);
                ReadParameters(context, c);
            }

            protected override void Initialize(SonarAnalysisContext context) =>
                throw new NotImplementedException();
        }
    }
}
