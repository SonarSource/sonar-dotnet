/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.Core.Syntax.Utilities.Test;

[TestClass]
public class GeneratedCodeRecognizerTest
{
    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    public void IsGenerated_WithNullOrEmptyPathAndNullRoot_ReturnsFalse(string path)
    {
        var tree = Substitute.For<SyntaxTree>();
        tree.FilePath.Returns(path);

        new TestRecognizer().IsGenerated(tree).Should().BeFalse();
    }

    [TestMethod]
    [DataRow(@"C:\SonarSource\SomeFile.g.cs")]
    [DataRow(@"C:\SonarSource\SomeFile_razor.g.cs")]
    [DataRow(@"C:\SonarSource\SomeFile_cshtml.g.cs")]
    [DataRow(@"C:\SonarSource\SomeFile_razor.ide.g.cs")]
    [DataRow(@"C:\SonarSource\SomeFile_cshtml.ide.g.cs")]
    [DataRow(@"C:\SonarSource\SomeFile_RAZOR.g.cS")]
    public void IsGenerated_GeneratedFiles_ReturnsTrue(string path)
    {
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

    [TestMethod]
    [DataRow(@"C:\SonarSource\SomeFile_razor.g.cs")]
    [DataRow(@"C:\SonarSource\SomeFile_RAZOR.g.cS")]
    [DataRow(@"C:\SonarSource\SomeFile_razor.ide.g.cs")]
    [DataRow(@"C:\SonarSource\SomeFile.razor.-6NXeWT5Akt4vxdz.ide.g.cs")]
    [DataRow(@"SomeFile_razor.ide.g.cs")]
    [DataRow(@"SomeFile.razor.-6NXeWT5Akt4vxdz.ide.g.cs")]
    public void IsRazorGeneratedFile_RazorGeneratedFiles_Razor_ReturnsTrue(string path)
    {
        var tree = Substitute.For<SyntaxTree>();
        tree.FilePath.Returns(path);

        GeneratedCodeRecognizer.IsRazorGeneratedFile(tree).Should().BeTrue();
        GeneratedCodeRecognizer.IsRazor(tree).Should().BeTrue();
        GeneratedCodeRecognizer.IsCshtml(tree).Should().BeFalse();
    }

    [TestMethod]
    [DataRow(@"C:\SonarSource\SomeFile_cshtml.g.cs")]
    [DataRow(@"C:\SonarSource\SomeFile_CSHTML.g.cs")]
    [DataRow(@"SomeFile_cshtml.g.cs")]
    [DataRow(@"SomeFile_cshtml.ide.g.cs")]
    [DataRow(@"C:\SonarSource\SomeFile_cshtml.ide.g.cs")]
    [DataRow(@"C:\SonarSource\SomeFile.cshtml.-6NXeWT5Akt4vxdz.ide.g.cs")]
    [DataRow(@"SomeFile.cshtml.-6NXeWT5Akt4vxdz.ide.g.cs")]
    public void IsRazorGeneratedFile_RazorGeneratedFiles_Cshtml_ReturnsTrue(string path)
    {
        var tree = Substitute.For<SyntaxTree>();
        tree.FilePath.Returns(path);

        GeneratedCodeRecognizer.IsRazorGeneratedFile(tree).Should().BeTrue();
        GeneratedCodeRecognizer.IsCshtml(tree).Should().BeTrue();
        GeneratedCodeRecognizer.IsRazor(tree).Should().BeFalse();
    }

    [TestMethod]
    [DataRow(@"C:\SonarSource\SomeFile.g.cs")]
    [DataRow(@"C:\SonarSource\SomeFile_razor.g.cs.randomEnding")]
    [DataRow(@"C:\SonarSource\SomeFile_cshtml.g.cs.randomEnding")]
    [DataRow(@"C:\SonarSource\SomeFile_razor.g.ß")]
    [DataRow(@"SomeFile.__virtual.cs")]
    [DataRow(@"SomeFile.razor__virtual.cs")]
    [DataRow(@"virtualcsharp-razor:///c:/dev/Project/Views/Home/Index.razor__viRtUaL.cs")]
    [DataRow(@"virtualcsharp-razor:///c:/dev/Project/Views/Home/Index.cshtml__VIRTUAL.cs")]
    [DataRow(@"SomeFile.php.-ABC.ide.g.cs")]
    public void IsRazorGeneratedFile_NonRazorGeneratedFiles_ReturnsFalse(string path)
    {
        var tree = Substitute.For<SyntaxTree>();
        tree.FilePath.Returns(path);

        GeneratedCodeRecognizer.IsRazorGeneratedFile(tree).Should().BeFalse();
        GeneratedCodeRecognizer.IsRazor(tree).Should().BeFalse();
        GeneratedCodeRecognizer.IsCshtml(tree).Should().BeFalse();
    }

    [TestMethod]
    [DataRow(@"C:\SonarSource\SomeFile_razor.g.cs")]
    [DataRow(@"C:\SonarSource\SomeFile_RAZOR.g.cS")]
    [DataRow(@"SomeFile_RAZOR.g.cS")]
    [DataRow(@"C:\SonarSource\SomeFile_cshtml.g.cs")]
    [DataRow(@"C:\SonarSource\SomeFile_CSHTML.g.cS")]
    [DataRow(@"SomeFile_csHTML.g.cS")]
    public void IsBuildTimeRazorGeneratedFile_ReturnsTrue(string path)
    {
        var tree = Substitute.For<SyntaxTree>();
        tree.FilePath.Returns(path);

        GeneratedCodeRecognizer.IsBuildTimeRazorGeneratedFile(tree).Should().BeTrue();
        GeneratedCodeRecognizer.IsDesignTimeRazorGeneratedFile(tree).Should().BeFalse();
    }

    [TestMethod]
    [DataRow(@"C:\SonarSource\SomeFile_razor.ide.g.cs")]
    [DataRow(@"C:\SonarSource\SomeFile.razor.-6NXeWT5Akt4vxdz.ide.g.cs")]
    [DataRow(@"SomeFile_razor.ide.g.cs")]
    [DataRow(@"SomeFile.razor.-6NXeWT5Akt4vxdz.ide.g.cs")]
    [DataRow(@"C:\SonarSource\SomeFile_cshtml.ide.g.cs")]
    [DataRow(@"C:\SonarSource\SomeFile.csHTml.-6NXeWT5Akt4vxdz.ide.g.cs")]
    [DataRow(@"SomeFile_cshtml.ide.g.cs")]
    [DataRow(@"SomeFile.CSHTmL.-6NXeWT5Akt4vxdz.ide.g.cs")]
    public void IsDesignTimeRazorGeneratedFile_ReturnsTrue(string path)
    {
        var tree = Substitute.For<SyntaxTree>();
        tree.FilePath.Returns(path);

        GeneratedCodeRecognizer.IsDesignTimeRazorGeneratedFile(tree).Should().BeTrue();
        GeneratedCodeRecognizer.IsBuildTimeRazorGeneratedFile(tree).Should().BeFalse();
    }

    [TestMethod]
    public void IsRazorGeneratedFile_NullSyntaxTree_ReturnsFalse() =>
        GeneratedCodeRecognizer.IsRazorGeneratedFile(null).Should().BeFalse();

    [TestMethod]
    public void IsBuildTimeRazorGeneratedFile_NullSyntaxTree_ReturnsFalse() =>
        GeneratedCodeRecognizer.IsBuildTimeRazorGeneratedFile(null).Should().BeFalse();

    [TestMethod]
    public void IsDesignTimeRazorGeneratedFile_NullSyntaxTree_ReturnsFalse() =>
        GeneratedCodeRecognizer.IsDesignTimeRazorGeneratedFile(null).Should().BeFalse();

#pragma warning disable T0016 // Internal Styling Rule T0016
    private class TestRecognizer : GeneratedCodeRecognizer
    {
        protected override string GetAttributeName(SyntaxNode node) => string.Empty;
        protected override bool IsTriviaComment(SyntaxTrivia trivia) => false;
    }
}
