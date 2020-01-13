/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2020 SonarSource SA
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
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Rules;

namespace SonarAnalyzer.UnitTest.Rules.Utilities
{
    [TestClass]
    public class UtilityAnalyzerBaseTests
    {
        private static DummySourceText DUMMY_TEXT = new DummySourceText();

        [DataTestMethod]
        [DataRow(LanguageNames.CSharp, "path\\output-cs")]
        [DataRow(LanguageNames.VisualBasic, "path\\output-vbnet")]
        [TestCategory("Utility")]
        public void UtilityAnalyzerBase_ReadParameters_OutputPath(string language, string expectedWorkDirectoryPath)
        {
            // we do not test what is read from the SonarLint file, but we need it
            var sonarLintFile = CreateMockAdditionalText(DUMMY_TEXT, "ResourceTests\\SonarLint.xml");
            // the output path is inside this file:
            var projectOutputFile = CreateMockAdditionalText(DUMMY_TEXT, "ResourceTests\\ProjectOutFolderPath.txt");
            var analyzerOptions = new AnalyzerOptions(ImmutableArray.Create(sonarLintFile.Object, projectOutputFile.Object));

            // Act
            var utilityAnalyzer = new TestUtilityAnalyzer();
            utilityAnalyzer.TestReadParameters(analyzerOptions, language);

            // Assert
            utilityAnalyzer.TestWorkDirectoryBasePath.Should().Be(expectedWorkDirectoryPath);
            utilityAnalyzer.TestIsAnalyzerEnabled.Should().BeTrue();
        }

        [DataTestMethod]
        [DataRow(LanguageNames.CSharp, "ResourceTests\\AnalyzeGeneratedTrue\\SonarLint.xml", true)]
        [DataRow(LanguageNames.CSharp, "ResourceTests\\AnalyzeGeneratedFalse\\SonarLint.xml", false)]
        [DataRow(LanguageNames.VisualBasic, "ResourceTests\\AnalyzeGeneratedTrueVbnet\\SonarLint.xml", true)]
        [DataRow(LanguageNames.VisualBasic, "ResourceTests\\AnalyzeGeneratedFalseVbnet\\SonarLint.xml", false)]
        [TestCategory("Utility")]
        public void UtilityAnalyzerBase_ReadsSettings_AnalyzeGenerated(string language, string sonarLintXmlPath, bool expectedAnalyzeGeneratedCodeValue)
        {
            var sonarLintFile = CreateMockAdditionalText(DUMMY_TEXT, sonarLintXmlPath);
            var projectOutputFile = CreateMockAdditionalText(DUMMY_TEXT, "ResourceTests\\ProjectOutFolderPath.txt");
            var analyzerOptions = new AnalyzerOptions(ImmutableArray.Create(sonarLintFile.Object, projectOutputFile.Object));

            // Act
            var utilityAnalyzer = new TestUtilityAnalyzer();
            utilityAnalyzer.TestReadParameters(analyzerOptions, language);

            // Assert
            utilityAnalyzer.TestAnalyzeGeneratedCode.Should().Be(expectedAnalyzeGeneratedCodeValue);
            utilityAnalyzer.TestIsAnalyzerEnabled.Should().BeTrue();
        }

        [DataTestMethod]
        [DataRow(LanguageNames.CSharp, UtilityAnalyzerBase.IgnoreHeaderCommentsCSharp, "ResourceTests\\IgnoreHeaderCommentsTrueCSharp\\SonarLint.xml", true)]
        [DataRow(LanguageNames.CSharp, UtilityAnalyzerBase.IgnoreHeaderCommentsCSharp, "ResourceTests\\IgnoreHeaderCommentsFalseCSharp\\SonarLint.xml", false)]
        [DataRow(LanguageNames.VisualBasic, UtilityAnalyzerBase.IgnoreHeaderCommentsVisualBasic, "ResourceTests\\IgnoreHeaderCommentsTrueVbnet\\SonarLint.xml", true)]
        [DataRow(LanguageNames.VisualBasic, UtilityAnalyzerBase.IgnoreHeaderCommentsVisualBasic, "ResourceTests\\IgnoreHeaderCommentsFalseVbnet\\SonarLint.xml", false)]
        [TestCategory("Utility")]
        public void UtilityAnalyzerBase_ReadsSettings_IgnoreHeaderComments(string language,
            string setting,
            string sonarLintXmlPath,
            bool expectedIgnoreHeaderComments)
        {
            var sonarLintFile = CreateMockAdditionalText(DUMMY_TEXT, sonarLintXmlPath);
            var projectOutputFile = CreateMockAdditionalText(DUMMY_TEXT, "ResourceTests\\ProjectOutFolderPath.txt");
            var analyzerOptions = new AnalyzerOptions(ImmutableArray.Create(sonarLintFile.Object, projectOutputFile.Object));

            // Act
            var utilityAnalyzer = new TestUtilityAnalyzer();
            utilityAnalyzer.TestReadParameters(analyzerOptions, language);

            // Assert
            utilityAnalyzer.TestIgnoreHeaderComments[setting].Should().Be(expectedIgnoreHeaderComments);
            utilityAnalyzer.TestIsAnalyzerEnabled.Should().BeTrue();
        }

        [TestMethod]
        [TestCategory("Utility")]
        public void UtilityAnalyzerBase_NoSonarLintXml_AnalyzerNotEnabled()
        {
            var projectOutputFile = CreateMockAdditionalText(DUMMY_TEXT, "ResourceTests\\ProjectOutFolderPath.txt");
            var analyzerOptions = new AnalyzerOptions(ImmutableArray.Create(projectOutputFile.Object));

            // Act
            var utilityAnalyzer = new TestUtilityAnalyzer();
            utilityAnalyzer.TestReadParameters(analyzerOptions, LanguageNames.CSharp);

            // Assert
            utilityAnalyzer.TestIsAnalyzerEnabled.Should().BeFalse();
        }

        [TestMethod]
        [TestCategory("Utility")]
        public void UtilityAnalyzerBase_NoOutputPath_AnalyzerNotEnabled()
        {
            var sonarLintFile = CreateMockAdditionalText(DUMMY_TEXT, "ResourceTests\\AnalyzeGeneratedTrue\\SonarLint.xml");
            var analyzerOptions = new AnalyzerOptions(ImmutableArray.Create(sonarLintFile.Object));

            // Act
            var utilityAnalyzer = new TestUtilityAnalyzer();
            utilityAnalyzer.TestReadParameters(analyzerOptions, LanguageNames.CSharp);

            // Assert
            utilityAnalyzer.TestIsAnalyzerEnabled.Should().BeFalse();
        }

        [TestMethod]
        [TestCategory("Utility")]
        public void UtilityAnalyzerBase_GetTextRange()
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

        [Microsoft.CodeAnalysis.Diagnostics.DiagnosticAnalyzer(LanguageNames.CSharp)]
        private class TestUtilityAnalyzer : UtilityAnalyzerBase
        {
            public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => throw new NotImplementedException();

            protected override void Initialize(SonarAnalysisContext context)
            {
                throw new NotImplementedException();
            }

            public void TestReadParameters(AnalyzerOptions options, string language) => this.ReadParameters(options, language);

            public bool TestIsAnalyzerEnabled
            {
                get
                {
                    return IsAnalyzerEnabled;
                }
            }

            public string TestWorkDirectoryBasePath
            {
                get
                {
                    return WorkDirectoryBasePath;
                }
            }

            public bool TestAnalyzeGeneratedCode
            {
                get
                {
                    return AnalyzeGeneratedCode;
                }
            }

            public Dictionary<string, bool> TestIgnoreHeaderComments
            {
                get
                {
                    return IgnoreHeaderComments;
                }
            }
        }

        private static Mock<AdditionalText> CreateMockAdditionalText(SourceText sourceText, string path)
        {
            var additionalTextMock = new Mock<AdditionalText>();
            additionalTextMock.Setup(x => x.Path).Returns(path);
            additionalTextMock.Setup(x => x.GetText(System.Threading.CancellationToken.None)).Returns(sourceText);
            return additionalTextMock;
        }

        // We can't use Mock<SourceText> because SourceText is an abstract class
        private class DummySourceText : SourceText
        {
            public override char this[int position] => throw new NotImplementedException();

            public override Encoding Encoding => throw new NotImplementedException();

            public override int Length => throw new NotImplementedException();

            public override void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count)
            {
                throw new NotImplementedException();
            }
        }
    }
}
