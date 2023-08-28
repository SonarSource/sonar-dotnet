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

using Moq;

namespace SonarAnalyzer.UnitTest.Helpers
{
    [TestClass]
    public class GeneratedCodeRecognizerTest
    {
        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        public void IsGenerated_WithNullOrEmptyPathAndNullRoot_ReturnsFalse(string path)
        {
            var syntaxTree = new Mock<SyntaxTree>(MockBehavior.Loose);
            syntaxTree.Setup(x => x.FilePath).Returns(path);

            new TestRecognizer().IsGenerated(syntaxTree.Object).Should().BeFalse();
        }

        [DataTestMethod]
        [DataRow("C:\\SonarSource\\SomeFile.g.cs")]
        [DataRow("C:\\SonarSource\\SomeFile_razor.g.cs")]
        [DataRow("C:\\SonarSource\\SomeFile_cshtml.g.cs")]
        [DataRow("C:\\SonarSource\\SomeFile_razor.ide.g.cs")]
        [DataRow("C:\\SonarSource\\SomeFile_cshtml.ide.g.cs")]
        [DataRow("C:\\SonarSource\\SomeFile_RAZOR.g.cS")]
        public void IsGenerated_GeneratedFiles_ReturnsTrue(string path)
        {
            // GetRoot() cannot be mocked - not virtual, so we use Loose behaviour to return null Root
            var syntaxTree = new Mock<SyntaxTree>(MockBehavior.Loose);
            syntaxTree.Setup(x => x.FilePath).Returns(path);
            new TestRecognizer().IsGenerated(syntaxTree.Object).Should().BeTrue();
        }

        [TestMethod]
        public void IsGenerated_NonGeneratedPath_ReturnsTrue()
        {
            var syntaxTree = new Mock<SyntaxTree>(MockBehavior.Loose);
            syntaxTree.Setup(x => x.FilePath).Returns("C:\\SonarSource\\SomeFile.cs");
            new TestRecognizer().IsGenerated(syntaxTree.Object).Should().BeFalse();
        }

        [DataTestMethod]
        [DataRow("C:\\SonarSource\\SomeFile_razor.g.cs")]
        [DataRow("C:\\SonarSource\\SomeFile_cshtml.g.cs")]
        [DataRow("C:\\SonarSource\\SomeFile_razor.ide.g.cs")]
        [DataRow("C:\\SonarSource\\SomeFile_cshtml.ide.g.cs")]
        [DataRow("C:\\SonarSource\\SomeFile_RAZOR.g.cS")]
        public void IsRazorGeneratedFile_RazorGeneratedFiles_ReturnsTrue(string path)
        {
            // GetRoot() cannot be mocked - not virtual, so we use Loose behaviour to return null Root
            var syntaxTree = new Mock<SyntaxTree>(MockBehavior.Loose);
            syntaxTree.Setup(x => x.FilePath).Returns(path);

            GeneratedCodeRecognizer.IsRazorGeneratedFile(syntaxTree.Object).Should().BeTrue();
        }

        [DataTestMethod]
        [DataRow("C:\\SonarSource\\SomeFile.g.cs")]
        [DataRow("C:\\SonarSource\\SomeFile_razor.g.cs.randomEnding")]
        [DataRow("C:\\SonarSource\\SomeFile_cshtml.g.cs.randomEnding")]
        [DataRow("C:\\SonarSource\\SomeFile_razor.g.ß")]
        public void IsRazorGeneratedFile_NonRazorGeneratedFiles_ReturnsFalse(string path)
        {
            var syntaxTree = new Mock<SyntaxTree>(MockBehavior.Loose);
            syntaxTree.Setup(x => x.FilePath).Returns(path);

            GeneratedCodeRecognizer.IsRazorGeneratedFile(syntaxTree.Object).Should().BeFalse();
        }

        [TestMethod]
        public void IsRazorGeneratedFile_NullSyntaxTree_ReturnsFalse()
        {
            var result = GeneratedCodeRecognizer.IsRazorGeneratedFile(null);

            result.Should().BeFalse();
        }

        private class TestRecognizer : GeneratedCodeRecognizer
        {
            protected override string GetAttributeName(SyntaxNode node) => string.Empty;
            protected override bool IsTriviaComment(SyntaxTrivia trivia) => false;
        }
    }
}
