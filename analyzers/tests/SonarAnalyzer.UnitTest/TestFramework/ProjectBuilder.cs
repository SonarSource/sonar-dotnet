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

using System.IO;
using System.Text;

namespace SonarAnalyzer.UnitTest.TestFramework
{
    internal readonly struct ProjectBuilder
    {
        private readonly Lazy<SolutionBuilder> solution;
        private readonly Project project;
        private readonly string fileExtension;

        public SolutionBuilder Solution => solution.Value;

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
                references = references.Concat(NetStandardMetadataReference.Netstandard);
            }
            var existingReferences = project.MetadataReferences.ToHashSet();
            return FromProject(project.AddMetadataReferences(references.Distinct().Where(x => !existingReferences.Contains(x))));
        }

        public ProjectBuilder AddProjectReference(Func<SolutionBuilder, ProjectId> getProjectId) =>
            FromProject(project.AddProjectReference(new ProjectReference(getProjectId(Solution))));

        public ProjectBuilder AddDocuments(IEnumerable<string> paths) =>
            paths.Aggregate(this, (projectBuilder, path) => projectBuilder.AddDocument(path));

        public ProjectBuilder AddDocument(string path)
        {
            _ = path ?? throw new ArgumentNullException(nameof(path));
            var fileInfo = new FileInfo(path);
            var relativePathFromTestCases = RelativePathFromTestCases(fileInfo);
            return fileInfo.Extension == fileExtension
                ? AddDocument(project, relativePathFromTestCases, File.ReadAllText(fileInfo.FullName, Encoding.UTF8))
                : throw new ArgumentException($"The file extension '{fileInfo.Extension}' does not match the project language '{project.Language}'.", nameof(path));

            static string RelativePathFromTestCases(FileInfo fileInfo)
            {
                Stack<string> directories = new();
                directories.Push(fileInfo.Name);
                var directory = fileInfo.Directory;
                while (directory != null && directory.Name.ToUpper() != "TESTCASES")
                {
                    directories.Push(directory.Name);
                    directory = directory.Parent;
                }
                return directory == null
                    ? throw new ArgumentException("path must contain 'TestCases'", nameof(fileInfo))
                    : Path.Combine(directories.ToArray());
            }
        }

        public ProjectBuilder AddSnippets(params string[] snippets) =>
            snippets.Aggregate(this, (current, snippet) => current.AddSnippet(snippet));

        public ProjectBuilder AddSnippet(string code, string fileName = null)
        {
            _ = code ?? throw new ArgumentNullException(nameof(code));
            fileName ??= $"snippet{project.Documents.Count()}{fileExtension}";
            return AddDocument(project, fileName, code);
        }

        public static ProjectBuilder FromProject(Project project) =>
            new(project);

        private static ProjectBuilder AddDocument(Project project, string fileName, string fileContent) =>
            FromProject(project.AddDocument(fileName, fileContent).Project);
    }
}
