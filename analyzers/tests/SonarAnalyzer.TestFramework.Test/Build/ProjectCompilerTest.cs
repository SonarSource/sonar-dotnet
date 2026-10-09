/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.TestFramework.Test.Build;

[TestClass]
public class ProjectCompilerTest
{
    private static readonly ProjectBuilder EmptyCS = SolutionBuilder.Create().AddProject(AnalyzerLanguage.CSharp);

    [TestMethod]
    public void Compile_NoRazorFiles() =>
        ProjectCompiler.Compile(EmptyCS.AddSnippet("public class Sample { }").Project).SyntaxTrees.Should().ContainSingle();

    [TestMethod]
    public void Compile_NonRazorAdditionalFile() =>
        ProjectCompiler.Compile(EmptyCS.Project.AddAdditionalDocument("extra.txt", "Not a Razor file").Project).SyntaxTrees.Should().BeEmpty();

    [TestMethod]
    public void Compile_RazorFile() =>
        ProjectCompiler.Compile(RazorProject(@"TestCases\ProjectCompiler.Component.razor")).SyntaxTrees.Should().Contain(x => x.FilePath.EndsWith("_razor.g.cs"));

    [TestMethod]
    public void Compile_CshtmlFile() =>
        ProjectCompiler.Compile(RazorProject(@"TestCases\ProjectCompiler.View.cshtml")).SyntaxTrees.Should().Contain(x => x.FilePath.EndsWith("_cshtml.g.cs"));

    [TestMethod]
    public void Compile_RazorFileWithoutReferences() =>
        ProjectCompiler.Compile(EmptyCS.AddAdditionalDocument(@"TestCases\ProjectCompiler.Component.razor").Project).SyntaxTrees.Should().Contain(x => x.FilePath.EndsWith("_razor.g.cs"));

    [TestMethod]
    public void Compile_ReferencesWithoutRazorFile() =>
        ProjectCompiler.Compile(RazorReferences(EmptyCS).Project).SyntaxTrees.Should().BeEmpty();

    private static Project RazorProject(string additionalDocumentPath) =>
        RazorReferences(EmptyCS.AddAdditionalDocument(additionalDocumentPath)).Project;

    private static ProjectBuilder RazorReferences(ProjectBuilder builder) =>
        builder
            .AddReferences(NuGetMetadataReference.MicrosoftAspNetCoreAppRef("7.0.17"))
            .AddReferences(NuGetMetadataReference.SystemTextEncodingsWeb("7.0.0"));
}
