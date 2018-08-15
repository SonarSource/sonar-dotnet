/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.Common;
using CS = Microsoft.CodeAnalysis.CSharp;
using VB = Microsoft.CodeAnalysis.VisualBasic;

namespace SonarAnalyzer.UnitTest.TestFramework
{
    public struct SolutionBuilder
    {
        private readonly Solution solution;

        public IReadOnlyList<ProjectId> ProjectIds => solution.ProjectIds;

        public ImmutableArray<ParseOptions> DefaultParseOptions { get; }

        private SolutionBuilder(Solution solution)
        {
            this.solution = solution;

            DefaultParseOptions = ImmutableArray.Create<ParseOptions>(
                new CS.CSharpParseOptions(CS.LanguageVersion.CSharp5),
                new CS.CSharpParseOptions(CS.LanguageVersion.CSharp6),
                new CS.CSharpParseOptions(CS.LanguageVersion.CSharp7),
                new VB.VisualBasicParseOptions(VB.LanguageVersion.VisualBasic12),
                new VB.VisualBasicParseOptions(VB.LanguageVersion.VisualBasic14),
                new VB.VisualBasicParseOptions(VB.LanguageVersion.VisualBasic15)
            );
        }

        public ProjectBuilder AddProject(string projectName, string fileExtension)
        {
            var language = fileExtension == GetFileExtension(AnalyzerLanguage.CSharp)
               ? AnalyzerLanguage.CSharp
               : AnalyzerLanguage.VisualBasic;

            return AddProject(projectName, language);
        }

        public ProjectBuilder AddProject(string projectName, AnalyzerLanguage language)
        {
            var project = solution.AddProject(projectName, projectName, GetLanguageName(language));

            return ProjectBuilder.FromProject(project)
                .AddReference(MetadataReferenceHelper.FromFrameworkAssembly("mscorlib.dll"))
                .AddReference(MetadataReferenceHelper.FromFrameworkAssembly("System.dll"))
                .AddReference(MetadataReferenceHelper.FromFrameworkAssembly("System.Core.dll"))

                // adding an extra file to the project
                // this won't trigger any issues, but it keeps a reference to the original ParseOption, so
                // if an analyzer/codefix changes the language version, Roslyn throws an ArgumentException
                .AddSnippet(string.Empty, fileName: "ExtraEmptyFile.g." + language.GetFileExtension());
        }

        private static string GetLanguageName(AnalyzerLanguage language) =>
            language == AnalyzerLanguage.CSharp
                ? LanguageNames.CSharp
                : LanguageNames.VisualBasic;

        public static SolutionBuilder Create() =>
            FromSolution(new AdhocWorkspace().CurrentSolution);

        public static SolutionBuilder FromSolution(Solution solution) =>
            new SolutionBuilder(solution);

        public IReadOnlyList<Compilation> Compile(IEnumerable<ParseOptions> parseOptions = null)
        {
            parseOptions = parseOptions ?? DefaultParseOptions;

            return solution
                .Projects
                .SelectMany(project => parseOptions
                    .Where(GetParseOptionsFilter(project.Language))
                    .Select(options => GetCompilation(project, options)))
                .ToList()
                .AsReadOnly();
        }
        private static Compilation GetCompilation(Project project, ParseOptions options) =>
            project
                .WithParseOptions(options)
                .GetCompilationAsync()
                .Result;

        private static Func<ParseOptions, bool> GetParseOptionsFilter(string language)
        {
            if (language == LanguageNames.CSharp)
            {
                return parseOptions => parseOptions is CS.CSharpParseOptions;
            }
            else if (language == LanguageNames.VisualBasic)
            {
                return parseOptions => parseOptions is VB.VisualBasicParseOptions;
            }
            throw new NotSupportedException($"Not supported language '{language}'");
        }

        private static string GetFileExtension(AnalyzerLanguage language) =>
            "." + language.GetFileExtension();
    }
}
