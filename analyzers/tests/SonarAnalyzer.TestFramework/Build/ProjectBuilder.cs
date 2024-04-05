/*
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
using System.Text;
using Microsoft.CodeAnalysis.Text;

namespace SonarAnalyzer.TestFramework.Build;

public readonly struct ProjectBuilder
{
    private readonly Lazy<SolutionBuilder> solution;
    private readonly Project project;
    private readonly string fileExtension;

    public SolutionBuilder Solution => solution.Value;
    public Project Project => project;

    private ProjectBuilder(Project project)
    {
        this.project = project;
        fileExtension = project.Language == LanguageNames.CSharp ? ".cs" : ".vb";
        solution = new Lazy<SolutionBuilder>(() => SolutionBuilder.FromSolution(project.Solution));
    }

    public Compilation GetCompilation(ParseOptions parseOptions = null, CompilationOptions compilationOptions = null)
    {
        var projectWithOptions = parseOptions == null ? project : project.WithParseOptions(parseOptions);
        var compilation = projectWithOptions.GetCompilationAsync().Result;
        return compilationOptions == null ? compilation : compilation.WithOptions(compilationOptions);
    }

    public Document FindDocument(string name) =>
        project.Documents.Single(d => d.Name == name);

    public ProjectBuilder AddReferences(IEnumerable<MetadataReference> references)
    {
        if (references == null || !references.Any())
        {
            return this;
        }
        if (references.Any(x => x.Display.Contains("\\netstandard")))
        {
            references = references.Concat(MetadataReferenceFacade.NetStandard);
        }
        var existingReferences = project.MetadataReferences.ToHashSet();
        return FromProject(project.AddMetadataReferences(references.Distinct().Where(x => !existingReferences.Contains(x))));
    }

    public ProjectBuilder AddProjectReference(Func<SolutionBuilder, ProjectId> getProjectId) =>
        FromProject(project.AddProjectReference(new ProjectReference(getProjectId(Solution))));

    public ProjectBuilder AddDocuments(IEnumerable<string> paths) =>
        paths.Aggregate(this, (projectBuilder, path) => projectBuilder.AddDocument(path));

    public ProjectBuilder AddAdditionalDocuments(IEnumerable<string> paths) =>
        paths.Aggregate(this, (projectBuilder, path) => projectBuilder.AddAdditionalDocument(path));

    public ProjectBuilder AddDocument(string path) =>
        AddDocument(project, GetTestCaseFileRelativePath(path), File.ReadAllText(path, Encoding.UTF8));

    public ProjectBuilder AddAdditionalDocument(string path) =>
        AddAdditionalDocument(project, GetTestCaseFileRelativePath(path), File.ReadAllText(path, Encoding.UTF8));

    public ProjectBuilder AddSnippets(params string[] snippets) =>
        snippets.Aggregate(this, (current, snippet) => current.AddSnippet(snippet));

    public ProjectBuilder AddSnippets(IEnumerable<Snippet> snippets) =>
        snippets.Aggregate(this, (current, snippet) => current.AddSnippet(snippet.Content, snippet.FileName));

    public ProjectBuilder AddSnippetAsAdditionalDocument(IEnumerable<Snippet> snippets) =>
        snippets.Aggregate(this, (current, snippet) => AddAdditionalDocument(current.Project, snippet.Content, snippet.FileName));

    public ProjectBuilder AddSnippet(string code, string fileName = null)
    {
        _ = code ?? throw new ArgumentNullException(nameof(code));
        fileName ??= $"snippet{project.Documents.Count()}{fileExtension}";
        return AddDocument(project, fileName, code);
    }

    public ProjectBuilder AddAnalyzerReferences(IEnumerable<AnalyzerFileReference> sourceGenerators)
    {
        _ = sourceGenerators ?? throw new ArgumentNullException(nameof(sourceGenerators));
        return FromProject(project.WithAnalyzerReferences(sourceGenerators));
    }

    public static ProjectBuilder FromProject(Project project) =>
        new(project);

    private string GetTestCaseFileRelativePath(string path)
    {
        const string TestCases = @"TestCases\";
        _ = path ?? throw new ArgumentNullException(nameof(path));
        var fileInfo = new FileInfo(path);
        var testCasesIndex = fileInfo.FullName.IndexOf(TestCases, StringComparison.Ordinal);
        var relativePathFromTestCases = testCasesIndex < 0
            ? throw new ArgumentException($"{nameof(path)} must contain '{TestCases}'", nameof(path))
            : fileInfo.FullName.Substring(testCasesIndex + TestCases.Length);

        if (!IsExtensionOfSupportedType(fileInfo))
        {
            throw new ArgumentException($"The file extension '{fileInfo.Extension}' does not match the project language '{project.Language}' nor Razor.", nameof(path));
        }

        return relativePathFromTestCases;
    }

    private bool IsExtensionOfSupportedType(FileInfo fileInfo) =>
        fileInfo.Extension.Equals(fileExtension, StringComparison.OrdinalIgnoreCase)
        || fileInfo.Extension.Equals(".razor", StringComparison.OrdinalIgnoreCase)
        || (fileInfo.Extension.Equals(".cshtml", StringComparison.OrdinalIgnoreCase) && project.Language == LanguageNames.CSharp);

    private static ProjectBuilder AddDocument(Project project, string fileName, string fileContent) =>
        FromProject(project.AddDocument(fileName, fileContent).Project);

    private static ProjectBuilder AddAdditionalDocument(Project project, string fileName, string fileContent) =>
        FromProject(project.AddAdditionalDocument(fileName, fileContent).Project);

    public ProjectBuilder AddAnalyzerConfigDocument(string editorConfigPath, string content) =>
        FromProject(project.AddAnalyzerConfigDocument(editorConfigPath, SourceText.From(content), filePath: editorConfigPath).Project);
}

public record Snippet(string Content, string FileName);
