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
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.UnitTest.Helpers;

namespace SonarAnalyzer.UnitTest.TestFramework
{
    internal class Verifier
    {
        private static readonly Regex ImportsRegexVB = new(@"^\s*Imports\s+.+$", RegexOptions.Multiline | RegexOptions.RightToLeft);
        private readonly VerifierBuilder builder;
        private readonly DiagnosticAnalyzer[] analyzers;
        private readonly AnalyzerLanguage language;
        private readonly string[] onlyDiagnosticIds;

        public Verifier(VerifierBuilder builder)
        {
            this.builder = builder ?? throw new ArgumentNullException(nameof(builder));
            onlyDiagnosticIds = builder.OnlyDiagnostics.Select(x => x.Id).ToArray();
            analyzers = builder.Analyzers.Select(x => x()).ToArray();
            if (!analyzers.Any())
            {
                throw new ArgumentException($"{nameof(builder.Analyzers)} cannot be empty. Use {nameof(VerifierBuilder)}<TAnalyzer> instead or add at least one analyzer using {nameof(builder)}.{nameof(builder.AddAnalyzer)}().");
            }
            if (analyzers.Any(x => x == null))
            {
                throw new ArgumentException("Analyzer instance cannot be null.");
            }
            var allLanguages = analyzers.SelectMany(x => x.GetType().GetCustomAttributes<DiagnosticAnalyzerAttribute>()).SelectMany(x => x.Languages).Distinct().ToArray();
            if (allLanguages.Length > 1)
            {
                throw new ArgumentException($"All {nameof(builder.Analyzers)} must declare the same language in their DiagnosticAnalyzerAttribute.");
            }
            language = AnalyzerLanguage.FromName(allLanguages.Single());
            if (!builder.Paths.Any() && !builder.Snippets.Any())
            {
                throw new ArgumentException($"{nameof(builder.Paths)} cannot be empty. Add at least one file using {nameof(builder)}.{nameof(builder.AddPaths)}() or {nameof(builder.AddSnippet)}().");
            }
            if (builder.Paths.FirstOrDefault(x => !Path.GetExtension(x).Equals(language.FileExtension, StringComparison.OrdinalIgnoreCase)) is { } unexpectedPath)
            {
                throw new ArgumentException($"Path '{unexpectedPath}' doesn't match {language.LanguageName} file extension '{language.FileExtension}'.");
            }
        }

        public void Verify()    // This should never has any arguments
        {
            foreach (var compilation in Compile())
            {
                DiagnosticVerifier.Verify(compilation, analyzers, builder.ErrorBehavior, builder.SonarProjectConfigPath, onlyDiagnosticIds);
            }
        }

        public void VerifyNoIssueReported()    // This should never has any arguments
        {
            foreach (var compilation in Compile())
            {
                foreach (var analyzer in analyzers)
                {
                    DiagnosticVerifier.VerifyNoIssueReported(compilation, analyzer, builder.ErrorBehavior, builder.SonarProjectConfigPath, onlyDiagnosticIds);
                }
            }
        }

        private IEnumerable<Compilation> Compile()
        {
            const string TestCases = "TestCases";
            using var scope = new EnvironmentVariableScope { EnableConcurrentAnalysis = builder.ConcurrentAnalysis };
            var basePath = Path.GetFullPath(builder.BasePath == null ? TestCases : Path.Combine(TestCases, builder.BasePath));
            var paths = builder.Paths.Select(x => Path.Combine(basePath, x)).ToArray();
            var project = SolutionBuilder.Create()
                .AddProject(language, true, builder.OutputKind)
                .AddDocuments(paths)
                .AddDocuments(builder.ConcurrentAnalysis && builder.AutogenerateConcurrentFiles ? CreateConcurrencyTest(paths) : Enumerable.Empty<string>())
                .AddSnippets(builder.Snippets.ToArray())
                .AddReferences(builder.References);
            return project.GetSolution().Compile(builder.ParseOptions.ToArray());
        }

        private IEnumerable<string> CreateConcurrencyTest(IEnumerable<string> paths)
        {
            foreach (var path in paths)
            {
                var newPath = Path.ChangeExtension(path, ".Concurrent" + language.FileExtension);
                var content = File.ReadAllText(path, Encoding.UTF8);
                File.WriteAllText(newPath, InsertConcurrentNamespace(content));
                yield return newPath;
            }
        }

        private string InsertConcurrentNamespace(string content)
        {
            return language.LanguageName switch
            {
                LanguageNames.CSharp => $"namespace AppendedNamespaceForConcurrencyTest {{ {content} {Environment.NewLine}}}",  // Last line can be a comment
                LanguageNames.VisualBasic => content.Insert(ImportsIndexVB(), "Namespace AppendedNamespaceForConcurrencyTest : ") + Environment.NewLine + " : End Namespace",
                _ => throw new UnexpectedLanguageException(language)
            };

            int ImportsIndexVB() =>
                ImportsRegexVB.Match(content) is { Success: true } match ? match.Index + match.Length + 1 : 0;
        }
    }
}
