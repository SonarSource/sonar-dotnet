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

namespace SonarAnalyzer.UnitTest.AnalysisContext;

public partial class SonarAnalysisContextTest
{
    [TestMethod]
    public void ShouldAnalyzeGenerated_NoSonarLintFile_ReturnsFalse()
    {
        var sonarLintXml = CreateSonarLintXml(true);
        var options = CreateOptions(sonarLintXml, @"ResourceTests\Foo.xml");
        var compilation = CreateDummyCompilation(AnalyzerLanguage.CSharp);

        CreateSut().ShouldAnalyzeGenerated(compilation, options).Should().BeFalse();
        sonarLintXml.ToStringCallCount.Should().Be(0, "this file doesn't have 'SonarLint.xml' name");
    }

    [TestMethod]
    public void ShouldAnalyzeGenerated_ResultIsCached()
    {
        var sonarLintXml = CreateSonarLintXml(true);
        var additionalText = MockAdditionalText(sonarLintXml);
        var options = new AnalyzerOptions(ImmutableArray.Create(additionalText.Object));
        var compilation = CreateDummyCompilation(AnalyzerLanguage.CSharp);
        var sut = CreateSut();

        // Call ShouldAnalyzeGenerated multiple times...
        sut.ShouldAnalyzeGenerated(compilation, options).Should().BeTrue();
        sut.ShouldAnalyzeGenerated(compilation, options).Should().BeTrue();
        sut.ShouldAnalyzeGenerated(compilation, options).Should().BeTrue();

        // GetText should be called every time ShouldAnalyzeGenerated is called...
        additionalText.Verify(x => x.GetText(It.IsAny<CancellationToken>()), Times.Exactly(3));
        sonarLintXml.ToStringCallCount.Should().Be(1); // ... but we should only try to read the file once
    }

    [TestMethod]
    public void ShouldAnalyzeGenerated_InvalidXmlInSonarLintFile_ReturnsFalse()
    {
        var sonarLintXml = new DummySourceText("Not valid xml");
        var options = CreateOptions(sonarLintXml);
        var compilation = CreateDummyCompilation(AnalyzerLanguage.CSharp);
        var sut = CreateSut();

        // 1. Read -> no error, false returned
        sut.ShouldAnalyzeGenerated(compilation, options).Should().BeFalse();
        sonarLintXml.ToStringCallCount.Should().Be(1); // should have attempted to read the file

        // 2. Read again to check that the load error doesn't prevent caching from working
        sut.ShouldAnalyzeGenerated(compilation, options).Should().BeFalse();
        sonarLintXml.ToStringCallCount.Should().Be(1); // should not have attempted to read the file again
    }

    [TestMethod]
    public void ShouldAnalyzeGenerated_CorrectSettingUsed_VB()
    {
        var sonarLintXml = CreateSonarLintXml(false);
        var options = CreateOptions(sonarLintXml);
        var compilationCS = CreateDummyCompilation(AnalyzerLanguage.CSharp);
        var compilationVB = CreateDummyCompilation(AnalyzerLanguage.VisualBasic);
        var sut = CreateSut();

        sut.ShouldAnalyzeGenerated(compilationCS, options).Should().BeFalse();
        sut.ShouldAnalyzeGenerated(compilationVB, options).Should().BeTrue();

        sonarLintXml.ToStringCallCount.Should().Be(2, "file should be read once per language");

        // Read again to check caching
        sut.ShouldAnalyzeGenerated(compilationVB, options).Should().BeTrue();

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

    private static Compilation CreateDummyCompilation(AnalyzerLanguage language) =>
        TestHelper.Compile(string.Empty, false, language).Model.Compilation;

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
