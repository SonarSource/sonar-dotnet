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
using Microsoft.CodeAnalysis;
using SonarAnalyzer.Common;

namespace SonarAnalyzer.UnitTest.TestFramework
{
    internal struct SolutionBuilder
    {
        private const string GeneratedAssemblyName = "project";

        private Solution Solution { get; }

        public IReadOnlyList<ProjectId> ProjectIds => Solution.ProjectIds;

        private SolutionBuilder(Solution solution)
        {
            Solution = solution;
        }

        public ProjectBuilder AddTestProject(AnalyzerLanguage language, bool createExtraEmptyFile = true)
        {
            return AddProject(language, $"{GeneratedAssemblyName}{ProjectIds.Count}.Tests", createExtraEmptyFile);
        }

        public ProjectBuilder AddProject(AnalyzerLanguage language, bool createExtraEmptyFile = true)
        {
            return AddProject(language, $"{GeneratedAssemblyName}{ProjectIds.Count}", createExtraEmptyFile);
        }

        private ProjectBuilder AddProject(AnalyzerLanguage language, string projectName, bool createExtraEmptyFile)
        {
            var languageName = language == AnalyzerLanguage.CSharp
                ? LanguageNames.CSharp
                : LanguageNames.VisualBasic;

            var project = Solution.AddProject(projectName, projectName, languageName);

            var projectBuilder = ProjectBuilder
                .FromProject(project)
                .AddReferences(FrameworkMetadataReference.Mscorlib)
                .AddReferences(FrameworkMetadataReference.System)
                .AddReferences(FrameworkMetadataReference.SystemCore)
                .AddReferences(FrameworkMetadataReference.SystemRuntime)
                .AddReferences(FrameworkMetadataReference.SystemGlobalization);

            if (language == AnalyzerLanguage.VisualBasic)
            {
                // Need a reference to the VB dll to be able to use the Module keyword
                projectBuilder = projectBuilder.AddReferences(FrameworkMetadataReference.MicrosoftVisualBasic);
            }

            if (createExtraEmptyFile)
            {
                // adding an extra file to the project
                // this won't trigger any issues, but it keeps a reference to the original ParseOption, so
                // if an analyzer/codefix changes the language version, Roslyn throws an ArgumentException
                projectBuilder = projectBuilder
                    .AddSnippet(string.Empty, fileName: "ExtraEmptyFile.g." + language.FileExtension);
            }

            return projectBuilder;
        }

        public static SolutionBuilder Create() =>
            FromSolution(new AdhocWorkspace().CurrentSolution);

        public static SolutionBuilder CreateSolutionFromPaths(IEnumerable<string> paths,
            IEnumerable<MetadataReference> additionalReferences = null)
        {
            if (paths == null ||
                !paths.Any())
            {
                throw new ArgumentException("Please specify at least one file path to analyze.", nameof(paths));
            }

            var extensions = paths.Select(path => Path.GetExtension(path)).Distinct().ToList();
            if (extensions.Count != 1)
            {
                throw new ArgumentException("Please use a collection of paths with the same extension", nameof(paths));
            }

            var project = Create()
                .AddProject(AnalyzerLanguage.FromPath(paths.First()))
                .AddDocuments(paths)
                .AddReferences(additionalReferences);

            if (additionalReferences != null &&
                additionalReferences.Any(r => r.Display.Contains("\\netstandard")))
            {
                project = project.AddReferences(FrameworkMetadataReference.Netstandard);
            }

            return project.GetSolution();
        }

        public static SolutionBuilder FromSolution(Solution solution) =>
            new SolutionBuilder(solution);

        public IReadOnlyList<Compilation> Compile(params ParseOptions[] parseOptions)
        {
            var options = ParseOptionsHelper.GetParseOptionsOrDefault(parseOptions);

            return Solution
                .Projects
                .SelectMany(project => options
                    .Where(ParseOptionsHelper.GetFilterByLanguage(project.Language))
                    .Select(o => GetCompilation(project, o)))
                .ToList()
                .AsReadOnly();
        }

        private static Compilation GetCompilation(Project project, ParseOptions options) =>
            project
                .WithParseOptions(options)
                .GetCompilationAsync()
                .Result;
    }
}
