﻿/*
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

using SonarAnalyzer.Core.Syntax.Utilities;

namespace  SonarAnalyzer.Core.Test.Syntax.Utilities;

[TestClass]
public class GeneratedCodeRecognizerTest
{
    [DataTestMethod]
    [DataRow(null)]
    [DataRow("")]
    public void IsGenerated_WithNullOrEmptyPathAndNullRoot_ReturnsFalse(string path)
    {
        var tree = Substitute.For<SyntaxTree>();
        tree.FilePath.Returns(path);

        new TestRecognizer().IsGenerated(tree).Should().BeFalse();
    }

    [DataTestMethod]
    [DataRow(@"C:\SonarSource\SomeFile.g.cs")]
    [DataRow(@"C:\SonarSource\SomeFile_razor.g.cs")]
    [DataRow(@"C:\SonarSource\SomeFile_cshtml.g.cs")]
    [DataRow(@"C:\SonarSource\SomeFile_razor.ide.g.cs")]
    [DataRow(@"C:\SonarSource\SomeFile_cshtml.ide.g.cs")]
    [DataRow(@"C:\SonarSource\SomeFile_RAZOR.g.cS")]
    public void IsGenerated_GeneratedFiles_ReturnsTrue(string path)
    {
        // GetRoot() cannot be mocked - not virtual, so we use Loose behaviour to return null Root
        var tree = Substitute.For<SyntaxTree>();
        tree.FilePath.Returns(path);
        new TestRecognizer().IsGenerated(tree).Should().BeTrue();
    }

    [TestMethod]
    public void IsGenerated_NonGeneratedPath_ReturnsTrue()
    {
        var tree = Substitute.For<SyntaxTree>();
        tree.FilePath.Returns(@"C:\SonarSource\SomeFile.cs");
        new TestRecognizer().IsGenerated(tree).Should().BeFalse();
    }

    [DataTestMethod]
    [DataRow(@"C:\SonarSource\SomeFile_razor.g.cs")]
    [DataRow(@"C:\SonarSource\SomeFile_cshtml.g.cs")]
    [DataRow(@"C:\SonarSource\SomeFile_RAZOR.g.cS")]
    public void IsRazorGeneratedFile_RazorGeneratedFiles_ReturnsTrue(string path)
    {
        // GetRoot() cannot be mocked - not virtual, so we use Loose behaviour to return null Root
        var tree = Substitute.For<SyntaxTree>();
        tree.FilePath.Returns(path);

        GeneratedCodeRecognizer.IsRazorGeneratedFile(tree).Should().BeTrue();
    }

    [DataTestMethod]
    [DataRow(@"C:\SonarSource\SomeFile.g.cs")]
    [DataRow(@"C:\SonarSource\SomeFile_razor.g.cs.randomEnding")]
    [DataRow(@"C:\SonarSource\SomeFile_cshtml.g.cs.randomEnding")]
    [DataRow(@"C:\SonarSource\SomeFile_razor.g.ß")]
    [DataRow(@"C:\SonarSource\SomeFile_razor.ide.g.cs")] // Not considered razor file because of https://github.com/dotnet/razor/issues/9108
    [DataRow(@"C:\SonarSource\SomeFile_cshtml.ide.g.cs")] // Not considered razor file because of https://github.com/dotnet/razor/issues/9108
    public void IsRazorGeneratedFile_NonRazorGeneratedFiles_ReturnsFalse(string path)
    {
        var tree = Substitute.For<SyntaxTree>();
        tree.FilePath.Returns(path);

        GeneratedCodeRecognizer.IsRazorGeneratedFile(tree).Should().BeFalse();
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
