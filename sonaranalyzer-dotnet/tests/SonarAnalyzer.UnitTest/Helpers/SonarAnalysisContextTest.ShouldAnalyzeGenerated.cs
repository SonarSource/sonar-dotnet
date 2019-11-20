/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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

extern alias csharp;
using System.Collections.Immutable;
using System.Text;
using System.Threading;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.UnitTest.Helpers
{
    [TestClass]
    public class SonarAnalysisContextTest_ShouldAnalyzeGenerated
    {
        [TestMethod]
        public void NoSonarLintFile_ReturnsFalse()
        {
            // Arrange
            var text = @"<?xml version='1.0' encoding='UTF-8'?>
<AnalysisInput>
  <Settings>
    <Setting>
      <Key>sonar.cs.analyzeGeneratedCode</Key>
      <Value>true</Value>
    </Setting>
  </Settings>
</AnalysisInput>
";
            var dummySourceText = new DummySourceText(text);
            var additionalTextMock = CreateMockAdditionalText(dummySourceText, "ResourceTests\\Foo.xml");
            var analyzerOptions = new AnalyzerOptions(ImmutableArray.Create(additionalTextMock.Object));

            var analysisContext = new Mock<AnalysisContext>();
            var compilation = GetDummyCompilation(isCSharp: true);

            // Act
            bool result = SonarAnalysisContext.ShouldAnalyzeGenerated(analysisContext.Object, compilation, analyzerOptions);

            // Assert
            result.Should().BeFalse();
            dummySourceText.ToStringCallCount.Should().Be(0); // Not the right file so shouldn't ever try to read it
        }

        [TestMethod]
        public void ResultIsCached()
        {
            // Arrange
            var text = @"<?xml version='1.0' encoding='UTF-8'?>
<AnalysisInput>
  <Settings>
    <Setting>
      <Key>dummy</Key>
      <Value>false</Value>
    </Setting>
    <Setting>
      <Key>sonar.cs.analyzeGeneratedCode</Key>
      <Value>true</Value>
    </Setting>
  </Settings>
</AnalysisInput>
";
            var dummySourceText = new DummySourceText(text);
            var additionalTextMock = CreateMockAdditionalText(dummySourceText, "ResourceTests\\SonarLint.xml");
            var analyzerOptions = new AnalyzerOptions(ImmutableArray.Create(additionalTextMock.Object));

            var analysisContext = new Mock<AnalysisContext>();
            var compilation = GetDummyCompilation(isCSharp: true);

            // Act - call ShouldAnalyzeGenerated multiple times...
            bool result = SonarAnalysisContext.ShouldAnalyzeGenerated(analysisContext.Object, compilation, analyzerOptions);
            result.Should().BeTrue();
            result = SonarAnalysisContext.ShouldAnalyzeGenerated(analysisContext.Object, compilation, analyzerOptions);
            result.Should().BeTrue();
            result = SonarAnalysisContext.ShouldAnalyzeGenerated(analysisContext.Object, compilation, analyzerOptions);
            result.Should().BeTrue();

            // Assert
            // GetText should be called every time ShouldAnalyzeGenerated is called...
            additionalTextMock.Verify(x => x.GetText(It.IsAny<CancellationToken>()), Times.Exactly(3));
            dummySourceText.ToStringCallCount.Should().Be(1); // ... but we should only try to read the file once
        }

        [TestMethod]
        public void InvalidXmlInSonarLintFile_ReturnsFalse()
        {
            // Arrange
            var dummySourceText = new DummySourceText("Not valid xml");
            var additionalTextMock = CreateMockAdditionalText(dummySourceText, "ResourceTests\\SonarLint.xml");
            var analyzerOptions = new AnalyzerOptions(ImmutableArray.Create(additionalTextMock.Object));

            var analysisContext = new Mock<AnalysisContext>();
            var compilation = GetDummyCompilation(isCSharp: true);

            // 1. Read -> no error, false returned
            bool result = SonarAnalysisContext.ShouldAnalyzeGenerated(analysisContext.Object, compilation, analyzerOptions);
            result.Should().BeFalse();
            dummySourceText.ToStringCallCount.Should().Be(1); // should have attempted to read the file

            // 2. Read again to check that the load error doesn't prevent caching from working
            result = SonarAnalysisContext.ShouldAnalyzeGenerated(analysisContext.Object, compilation, analyzerOptions);
            result.Should().BeFalse();
            dummySourceText.ToStringCallCount.Should().Be(1); // should not have attempted to read the file again
        }

        [TestMethod]
        public void VB_CorrectSettingUsed()
        {
            // Arrange
            var text = @"<?xml version='1.0' encoding='UTF-8'?>
<AnalysisInput>
  <Settings>
    <Setting>
      <Key>sonar.cs.analyzeGeneratedCode</Key>
      <Value>false</Value>
    </Setting>
    <Setting>
      <Key>sonar.vbnet.analyzeGeneratedCode</Key>
      <Value>true</Value>
    </Setting>
  </Settings>
</AnalysisInput>
";

            var dummySourceText = new DummySourceText(text);
            var additionalTextMock = CreateMockAdditionalText(dummySourceText, "ResourceTests\\SonarLint.xml");
            var analyzerOptions = new AnalyzerOptions(ImmutableArray.Create(additionalTextMock.Object));

            var analysisContext = new Mock<AnalysisContext>();
            var vbCompilation = GetDummyCompilation(isCSharp: false);
            var cSharpCompilation = GetDummyCompilation(isCSharp: true);

            // 1. Read both languages
            bool vbResult = SonarAnalysisContext.ShouldAnalyzeGenerated(analysisContext.Object, vbCompilation, analyzerOptions);
            bool csharpResult = SonarAnalysisContext.ShouldAnalyzeGenerated(analysisContext.Object, cSharpCompilation, analyzerOptions);

            // Assert
            vbResult.Should().BeTrue();
            csharpResult.Should().BeFalse();
            dummySourceText.ToStringCallCount.Should().Be(2); // file read once per language

            // 2. Read again for VB to check VB caching
            vbResult = SonarAnalysisContext.ShouldAnalyzeGenerated(analysisContext.Object, vbCompilation, analyzerOptions);

            // Assert
            vbResult.Should().BeTrue();
            dummySourceText.ToStringCallCount.Should().Be(2); // file should not have been read again
        }

        private static Mock<AdditionalText> CreateMockAdditionalText(SourceText sourceText, string path)
        {
            var additionalTextMock = new Mock<AdditionalText>();
            additionalTextMock.Setup(x => x.Path).Returns(path);
            additionalTextMock.Setup(x => x.GetText(System.Threading.CancellationToken.None)).Returns(sourceText);
            return additionalTextMock;
        }

        private static Compilation GetDummyCompilation(bool isCSharp)
        {
            (var syntaxTree, var semanticModel) = TestHelper.Compile(string.Empty, isCSharp);
            return semanticModel.Compilation;
        }

        // We can't use Mock<SourceText> because SourceText is an abstract class
        private class DummySourceText : SourceText
        {
            private readonly string textToReturn;
            public int ToStringCallCount { get; private set; }

            public DummySourceText(string textToReturn)
            {
                this.textToReturn = textToReturn;
            }

            public override string ToString()
            {
                this.ToStringCallCount++;
                return textToReturn;
            }

            #region Abstract methods

            public override char this[int position] => throw new System.NotImplementedException();

            public override Encoding Encoding => throw new System.NotImplementedException();

            public override int Length => throw new System.NotImplementedException();

            public override void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count)
            {
                throw new System.NotImplementedException();
            }

            #endregion
        }
    }
}
