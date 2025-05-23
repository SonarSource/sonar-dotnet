﻿/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

using System.IO;

namespace SonarAnalyzer.Test.TestFramework.Tests.Build;

[TestClass]
public class ProjectBuilderTest
{
    private static readonly ProjectBuilder EmptyCS = SolutionBuilder.Create().AddProject(AnalyzerLanguage.CSharp);
    private static readonly ProjectBuilder EmptyVB = SolutionBuilder.Create().AddProject(AnalyzerLanguage.VisualBasic);

    [TestMethod]
    public void AddDocument_Null_Throws() =>
        EmptyCS.Invoking(x => x.AddDocument(null)).Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("path");

    [TestMethod]
    public void AddDocument_NoTestCases_Throws() =>
        EmptyCS.Invoking(x => x.AddDocument("FileOutsideExpectedDirectory.cs")).Should()
            .Throw<ArgumentException>()
            .WithMessage(@"path must contain 'TestCases\'*")
            .Which.ParamName.Should().Be("path");

    [TestMethod]
    public void AddDocument_UnsupportedExtension_Throws() =>
        EmptyCS.Invoking(x => x.AddDocument(@"TestCases\File.vb")).Should()
            .Throw<ArgumentException>()
            .WithMessage("The file extension '.vb' does not match the project language 'C#' nor Razor.*")
            .Which.ParamName.Should().Be("path");

    [TestMethod]
    public void AddDocument_ValidExtension()
    {
        EmptyCS.AddDocument(@"TestCases\ProjectBuilder.AddDocument.cs").FindDocument("ProjectBuilder.AddDocument.cs").Should().NotBeNull();
        EmptyVB.AddDocument(@"TestCases\ProjectBuilder.AddDocument.vb").FindDocument("ProjectBuilder.AddDocument.vb").Should().NotBeNull();
    }

    [TestMethod]
    public void AddDocument_MismatchingExtension()
    {
        EmptyCS.Invoking(x => x.AddDocument(@"TestCases\ProjectBuilder.AddDocument.vb")).Should().Throw<ArgumentException>();
        EmptyVB.Invoking(x => x.AddDocument(@"TestCases\ProjectBuilder.AddDocument.cs")).Should().Throw<ArgumentException>();
    }

    [TestMethod]
    public void AddDocument_InvalidExtension()
    {
        EmptyCS.Invoking(x => x.AddDocument(@"TestCases\ProjectBuilder.AddDocument.unknown")).Should().Throw<ArgumentException>();
        EmptyVB.Invoking(x => x.AddDocument(@"TestCases\ProjectBuilder.AddDocument.unknown")).Should().Throw<ArgumentException>();
    }

    [TestMethod]
    public void AddAdditionalDocument_SupportsRazorFiles() =>
        AssertAdditionalDocumentContains(EmptyCS.AddAdditionalDocument(@"TestCases\ProjectBuilder.AddDocument.razor"), "ProjectBuilder.AddDocument.razor");

    [TestMethod]
    public void AddAdditionalDocument_CsharpSupportsCshtmlFiles() =>
        AssertAdditionalDocumentContains(EmptyCS.AddAdditionalDocument(@"TestCases\ProjectBuilder.AddDocument.cshtml"), "ProjectBuilder.AddDocument.cshtml");

    [DataTestMethod]
    [DataRow("cshtml")]
    [DataRow("razor")]
    public void AddAdditionalDocument_VbnetDoesntSupportRazorFiles(string extension) =>
        EmptyVB.Invoking(x => x.AddAdditionalDocument(@$"TestCases\ProjectBuilder.AddDocument.{extension}")).Should().Throw<ArgumentException>();

    [TestMethod]
    public void AddAdditionalDocument_CsharpDoesntSupportVbhtmlFiles() =>
        EmptyCS.Invoking(x => x.AddAdditionalDocument(@"TestCases\ProjectBuilder.AddDocument.vbhtml")).Should().Throw<ArgumentException>();

    [TestMethod]
    public void AddAdditionalDocument_VbnetDoesntSupportVbhtmlFiles() =>
        EmptyVB.Invoking(x => x.AddAdditionalDocument(@"TestCases\ProjectBuilder.AddDocument.vbhtml")).Should().Throw<ArgumentException>();

    [TestMethod]
    public void AddSnippet_Null_Throws() =>
        EmptyCS.Invoking(x => x.AddSnippet(null)).Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("code");

    [TestMethod]
    public void AddAnalyzerReferences_Null_Throws() =>
        EmptyCS.Invoking(x => x.AddAnalyzerReferences(null)).Should().Throw<ArgumentNullException>().Which.ParamName.Should().Be("sourceGenerators");

    [TestMethod]
    public void AddAnalyzerReferences_NullAnalyzer_Throws() =>
        EmptyCS.Invoking(x => x.AddAnalyzerReferences([null])).Should().Throw<ArgumentNullException>();

    [TestMethod]
    public void AddAnalyzerReferences_AddAnalyzer() =>
        EmptyCS.AddAnalyzerReferences(SourceGeneratorProvider.SourceGenerators).Project.AnalyzerReferences.Should().BeEquivalentTo(SourceGeneratorProvider.SourceGenerators);

    private static void AssertAdditionalDocumentContains(ProjectBuilder builder, string fileName) =>
        builder.Project.AdditionalDocuments.Should().ContainSingle(x => x.Name == Path.Combine(Directory.GetCurrentDirectory(), "TestCases", fileName));

    [TestMethod]
    public void AddAnalyzerConfigDocument_ShouldAddDocumentToProject() =>
        EmptyCS.AddAnalyzerConfigDocument("path/to/config.editorconfig", "root = true").Project
            .AnalyzerConfigDocuments.SingleOrDefault(d => d.Name == "path/to/config.editorconfig").Should().NotBeNull();
}
