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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;

namespace SonarAnalyzer.UnitTest.TestFramework
{
    internal struct ProjectBuilder
    {
        private const string FIXED_MESSAGE = "Fixed";

        private readonly Lazy<SolutionBuilder> solutionWrapper;

        private Project Project { get; }

        private string FileExtension { get; }

        private ProjectBuilder(Project project)
        {
            Project = project;
            FileExtension = project.Language == LanguageNames.CSharp ? ".cs" : ".vb";

            this.solutionWrapper = new Lazy<SolutionBuilder>(() => SolutionBuilder.FromSolution(project.Solution));
        }

        public SolutionBuilder GetSolution() =>
            this.solutionWrapper.Value;

        public Compilation GetCompilation(ParseOptions parseOptions = null, CompilationOptions compilationOptions = null)
        {
            var project = parseOptions != null
                ? Project.WithParseOptions(parseOptions)
                : Project;

            var compilation = project.GetCompilationAsync().Result;

            return compilationOptions == null
                ? compilation
                : compilation.WithOptions(compilationOptions);
        }

        public Document FindDocument(string name) =>
            Project.Documents.Single(d => d.Name == name);

        public ProjectBuilder AddReferences(IEnumerable<MetadataReference> references) =>
            FromProject(Project.AddMetadataReferences(references ?? Enumerable.Empty<MetadataReference>()));

        public ProjectBuilder AddProjectReference(Func<SolutionBuilder, ProjectId> getProjectId) =>
            FromProject(Project.AddProjectReference(new ProjectReference(getProjectId(GetSolution()))));

        public ProjectBuilder AddProjectReferences(Func<SolutionBuilder, IEnumerable<ProjectId>> getProjectIds) =>
            FromProject(Project.AddProjectReferences(getProjectIds(GetSolution()).Select(id => new ProjectReference(id))));

        public ProjectBuilder AddDocuments(IEnumerable<string> paths) =>
            paths.Aggregate(this, (projectBuilder, path) => projectBuilder.AddDocument(path));

        public ProjectBuilder AddDocument(string path, bool removeAnalysisComments = false)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            var fileInfo = new FileInfo(path);

            if (fileInfo.Extension != FileExtension)
            {
                throw new ArgumentException($"The file extension '{fileInfo.Extension}' does not" +
                    $" match the project language '{Project.Language}'.", nameof(path));
            }

            return AddDocument(Project, fileInfo.Name, File.ReadAllText(fileInfo.FullName, Encoding.UTF8), removeAnalysisComments);
        }

        public ProjectBuilder AddSnippet(string code, string fileName = null, bool removeAnalysisComments = false)
        {
            if (code == null)
            {
                throw new ArgumentNullException(nameof(code));
            }

            fileName = fileName ?? $"snippet{Project.Documents.Count()}{FileExtension}";

            return AddDocument(Project, fileName, code, removeAnalysisComments);
        }

        private static ProjectBuilder AddDocument(Project project, string fileName, string fileContent, bool removeAnalysisComments)
        {
            return FromProject(project.AddDocument(fileName, ReadDocument()).Project);

            string ReadDocument()
            {
                const string WindowsLineEnding = "\r\n";
                const string UnixLineEnding = "\n";

                var lines = fileContent
                    .Replace(WindowsLineEnding, UnixLineEnding) // This allows to deal with multiple line endings
                    .Split(new[] { UnixLineEnding }, StringSplitOptions.None);

                if (removeAnalysisComments)
                {
                    lines = lines.Where(IssueLocationCollector.IsNotIssueLocationLine)
                        .Select(ReplaceNonCompliantComment)
                        .ToArray();
                }

                return string.Join(UnixLineEnding, lines);
            }
        }

        private static string ReplaceNonCompliantComment(string line)
        {
            var match = Regex.Match(line, IssueLocationCollector.ISSUE_LOCATION_PATTERN);
            if (!match.Success)
            {
                return line;
            }

            if (match.Groups["issueType"].Value == "Noncompliant")
            {
                var startIndex = line.IndexOf(match.Groups["issueType"].Value);
                return string.Concat(line.Remove(startIndex), FIXED_MESSAGE);
            }

            return line.Replace(match.Value, string.Empty).TrimEnd();
        }

        public static ProjectBuilder FromProject(Project project) =>
            new ProjectBuilder(project);
    }
}
