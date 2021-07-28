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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.UnitTest.MetadataReferences;

namespace SonarAnalyzer.UnitTest.TestFramework
{
    internal struct ProjectBuilder
    {
        private const string FixedMessage = "Fixed";

        private readonly Lazy<SolutionBuilder> solutionWrapper;

        private Project Project { get; }

        private string FileExtension { get; }

        private ProjectBuilder(Project project)
        {
            Project = project;
            FileExtension = project.Language == LanguageNames.CSharp ? ".cs" : ".vb";

            solutionWrapper = new Lazy<SolutionBuilder>(() => SolutionBuilder.FromSolution(project.Solution));
        }

        public SolutionBuilder GetSolution() =>
            solutionWrapper.Value;

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

        public ProjectBuilder AddTestReferences() =>
            AddReferences(NuGetMetadataReference.MSTestTestFrameworkV1);    // Any reference to detect a test project

        public ProjectBuilder AddReferences(IEnumerable<MetadataReference> references)
        {
            var existingReferences = Project.MetadataReferences;
            var deduplicated = references == null
                ? Enumerable.Empty<MetadataReference>()
                : references.Where(mr => !existingReferences.Contains(mr)).Distinct().ToHashSet();
            return FromProject(Project.AddMetadataReferences(deduplicated));
        }

        public ProjectBuilder AddProjectReference(Func<SolutionBuilder, ProjectId> getProjectId) =>
            FromProject(Project.AddProjectReference(new ProjectReference(getProjectId(GetSolution()))));

        public ProjectBuilder AddDocuments(IEnumerable<string> paths) =>
            paths.Aggregate(this, (projectBuilder, path) => projectBuilder.AddDocument(path));

        public ProjectBuilder AddDocuments(IEnumerable<ProjectFileAsPathAndContent> paths) =>
            paths.Aggregate(this, (projectBuilder, path) => AddDocument(projectBuilder.Project,
                                                                        new FileInfo(path.Path).Name,
                                                                        path.Content,
                                                                        false));
        public ProjectBuilder AddDocument(string path, bool removeAnalysisComments = false)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            var fileInfo = new FileInfo(path);

            return fileInfo.Extension != FileExtension
                ? throw new ArgumentException($"The file extension '{fileInfo.Extension}' does not" +
                                              $" match the project language '{Project.Language}'.", nameof(path))
                : AddDocument(Project, fileInfo.Name, File.ReadAllText(fileInfo.FullName, Encoding.UTF8), removeAnalysisComments);
        }

        public ProjectBuilder AddSnippet(string code, string fileName = null, bool removeAnalysisComments = false)
        {
            if (code == null)
            {
                throw new ArgumentNullException(nameof(code));
            }

            fileName ??= $"snippet{Project.Documents.Count()}{FileExtension}";

            return AddDocument(Project, fileName, code, removeAnalysisComments);
        }

        public static ProjectBuilder FromProject(Project project) =>
            new ProjectBuilder(project);

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
                    lines = lines.Where(line => !IssueLocationCollector.RxPreciseLocation.IsMatch(line))
                        .Select(ReplaceNonCompliantComment)
                        .ToArray();
                }

                return string.Join(UnixLineEnding, lines);
            }
        }

        private static string ReplaceNonCompliantComment(string line)
        {
            var match = IssueLocationCollector.RxIssue.Match(line);
            if (!match.Success)
            {
                return line;
            }

            if (match.Groups["issueType"].Value == "Noncompliant")
            {
                var startIndex = line.IndexOf(match.Groups["issueType"].Value);
                return string.Concat(line.Remove(startIndex), FixedMessage);
            }

            return line.Replace(match.Value, string.Empty).TrimEnd();
        }
    }
}
