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

using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SonarAnalyzer.Helpers;

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
            // GetRoot() cannot be mocked - not virtual, so we use Loose behaviour to return null Root
            var syntaxTree = new Mock<SyntaxTree>(MockBehavior.Loose);
            syntaxTree.Setup(x => x.FilePath).Returns(path);

            var sut = new TestRecognizer();
            var result = sut.IsGenerated(syntaxTree.Object);

            result.Should().BeFalse();
        }

        private class TestRecognizer : GeneratedCodeRecognizer
        {
            protected override string GetAttributeName(SyntaxNode node) => "";
            protected override bool IsTriviaComment(SyntaxTrivia trivia) => false;
        }
    }
}
