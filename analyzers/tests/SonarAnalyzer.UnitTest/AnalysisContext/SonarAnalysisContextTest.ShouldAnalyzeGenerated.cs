/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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

using System.Text;
using Microsoft.CodeAnalysis.Text;
using Moq;
using SonarAnalyzer.Common;

namespace SonarAnalyzer.UnitTest;

public partial class SonarAnalysisContextTest   // FIXME: Move UTs to ShouldAnalyze
{
    private const string GeneratedFileName = "ExtraEmptyFile.g.";
    private const string OtherFileName = "OtherFile";

    [DataTestMethod]
    [DataRow(GeneratedFileName, false)]
    [DataRow(OtherFileName, true)]
    public void ShouldAnalyze_GeneratedFile_NoSonarLintXml(string fileName, bool expected)
    {
        var sonarLintXml = CreateSonarLintXml(true);
        var options = CreateOptions(sonarLintXml, @"ResourceTests\Foo.xml");
        var (compilation, tree) = CreateDummyCompilation(AnalyzerLanguage.CSharp, fileName);

        CreateSut().ShouldAnalyze(CSharpGeneratedCodeRecognizer.Instance, tree, compilation, options).Should().Be(expected);
        sonarLintXml.ToStringCallCount.Should().Be(0, "this file doesn't have 'SonarLint.xml' name");
    }

    [TestMethod]
    public void ShouldAnalyze_GeneratedFile_ShouldAnalyzeGeneratedProvider_IsCached()
    {
        var sonarLintXml = CreateSonarLintXml(true);
        var additionalText = MockAdditionalText(sonarLintXml);
        var options = new AnalyzerOptions(ImmutableArray.Create(additionalText.Object));
        var (compilation, tree) = CreateDummyCompilation(AnalyzerLanguage.CSharp, OtherFileName);
        var sut = CreateSut();

        // Call ShouldAnalyzeGenerated multiple times...
        sut.ShouldAnalyze(CSharpGeneratedCodeRecognizer.Instance, tree, compilation, options).Should().BeTrue();
        sut.ShouldAnalyze(CSharpGeneratedCodeRecognizer.Instance, tree, compilation, options).Should().BeTrue();
        sut.ShouldAnalyze(CSharpGeneratedCodeRecognizer.Instance, tree, compilation, options).Should().BeTrue();

        // GetText should be called every time ShouldAnalyzeGenerated is called...
        additionalText.Verify(x => x.GetText(It.IsAny<CancellationToken>()), Times.Exactly(3));
        sonarLintXml.ToStringCallCount.Should().Be(1); // ... but we should only try to read the file once
    }

    [DataTestMethod]
    [DataRow(GeneratedFileName, false)]
    [DataRow(OtherFileName, true)]
    public void ShouldAnalyze_GeneratedFile_InvalidSonarLintXml(string fileName, bool expected)
    {
        var sonarLintXml = new DummySourceText("Not valid xml");
        var options = CreateOptions(sonarLintXml);
        var (compilation, tree) = CreateDummyCompilation(AnalyzerLanguage.CSharp, fileName);
        var sut = CreateSut();

        // 1. Read -> no error
        sut.ShouldAnalyze(CSharpGeneratedCodeRecognizer.Instance, tree, compilation, options).Should().Be(expected);
        sonarLintXml.ToStringCallCount.Should().Be(1); // should have attempted to read the file

        // 2. Read again to check that the load error doesn't prevent caching from working
        sut.ShouldAnalyze(CSharpGeneratedCodeRecognizer.Instance, tree, compilation, options).Should().Be(expected);
        sonarLintXml.ToStringCallCount.Should().Be(1); // should not have attempted to read the file again
    }

    [DataTestMethod]
    [DataRow(GeneratedFileName)]
    [DataRow(OtherFileName)]
    public void ShouldAnalyze_GeneratedFile_AnalyzeGenerated_AnalyzeAllFiles(string fileName)
    {
        var sonarLintXml = CreateSonarLintXml(true);
        var options = CreateOptions(sonarLintXml);
        var (compilation, tree) = CreateDummyCompilation(AnalyzerLanguage.CSharp, fileName);
        var sut = CreateSut();

        sut.ShouldAnalyze(CSharpGeneratedCodeRecognizer.Instance, tree, compilation, options).Should().BeTrue();
    }

    [DataTestMethod]
    [DataRow(GeneratedFileName, false)]
    [DataRow(OtherFileName, true)]
    public void ShouldAnalyze_CorrectSettingUsed_VB(string fileName, bool expectedCSharp)
    {
        var sonarLintXml = CreateSonarLintXml(false);
        var options = CreateOptions(sonarLintXml);
        var (compilationCS, treeCS) = CreateDummyCompilation(AnalyzerLanguage.CSharp, fileName);
        var (compilationVB, treeVB) = CreateDummyCompilation(AnalyzerLanguage.VisualBasic, fileName);
        var sut = CreateSut();

        sut.ShouldAnalyze(CSharpGeneratedCodeRecognizer.Instance, treeCS, compilationCS, options).Should().Be(expectedCSharp);
        sut.ShouldAnalyze(VisualBasicGeneratedCodeRecognizer.Instance, treeVB, compilationVB, options).Should().BeTrue();

        sonarLintXml.ToStringCallCount.Should().Be(2, "file should be read once per language");

        // Read again to check caching
        sut.ShouldAnalyze(VisualBasicGeneratedCodeRecognizer.Instance, treeVB, compilationVB, options).Should().BeTrue();

        sonarLintXml.ToStringCallCount.Should().Be(2, "file should not have been read again");
    }

    private static SonarAnalysisContext CreateSut() =>
        new(Mock.Of<AnalysisContext>(), Enumerable.Empty<DiagnosticDescriptor>());

    private static DummySourceText CreateSonarLintXml(bool analyzeGeneratedCSharp) =>
        new($"""
            <?xml version="1.0" encoding="UTF-8"?>
            <AnalysisInput>
                <Settings>
                    <Setting>
                        <Key>dummy</Key>
                        <Value>false</Value>
                    </Setting>
                    <Setting>
                        <Key>sonar.cs.analyzeGeneratedCode</Key>
                        <Value>{analyzeGeneratedCSharp.ToString().ToLower()}</Value>
                    </Setting>
                    <Setting>
                        <Key>sonar.vbnet.analyzeGeneratedCode</Key>
                        <Value>true</Value>
                    </Setting>
                </Settings>
            </AnalysisInput>
            """);

    private static AnalyzerOptions CreateOptions(SourceText sourceText, string path = @"ResourceTests\SonarLint.xml") =>
        new(ImmutableArray.Create(MockAdditionalText(sourceText, path).Object));

    private static Mock<AdditionalText> MockAdditionalText(SourceText sourceText, string path = @"ResourceTests\SonarLint.xml")
    {
        var additionalText = new Mock<AdditionalText>();
        additionalText.Setup(x => x.Path).Returns(path);
        additionalText.Setup(x => x.GetText(default)).Returns(sourceText);
        return additionalText;
    }

    private static (Compilation Compilation, SyntaxTree Tree) CreateDummyCompilation(AnalyzerLanguage language, string treeFileName)
    {
        var compilation = SolutionBuilder.Create().AddProject(language).AddSnippet(string.Empty, OtherFileName + language.FileExtension).GetCompilation();
        return (compilation, compilation.SyntaxTrees.Single(x => x.FilePath.Contains(treeFileName)));
    }

    private sealed class DummySourceText : SourceText
    {
        private readonly string textToReturn;

        public int ToStringCallCount { get; private set; }
        public override char this[int position] => throw new NotImplementedException();
        public override Encoding Encoding => throw new NotImplementedException();
        public override int Length => throw new NotImplementedException();

        public DummySourceText(string textToReturn) =>
            this.textToReturn = textToReturn;

        public override string ToString()
        {
            ToStringCallCount++;
            return textToReturn;
        }

        public override void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count) =>
            throw new NotImplementedException();
    }
}
