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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.UnitTest.Helpers;

namespace SonarAnalyzer.UnitTest.TestFramework
{
    internal class Verifier
    {
        private static readonly Regex ImportsRegexVB = new(@"^\s*Imports\s+.+$", RegexOptions.Multiline | RegexOptions.RightToLeft);
        private readonly VerifierBuilder builder;
        private readonly DiagnosticAnalyzer[] analyzers;
        private readonly SonarCodeFixProvider codeFix;
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
            foreach (var path in builder.Paths)
            {
                ValidateExtension(path);
            }
            if (builder.CodeFix is not null)
            {
                codeFix = builder.CodeFix();
                _ = builder.CodeFixedPath ?? throw new ArgumentException($"{nameof(builder.CodeFixedPath)} was not set.");
                if (builder.Analyzers.Length != 1)
                {
                    throw new ArgumentException($"{nameof(builder.Analyzers)} must contain only 1 analyzer, but {analyzers.Length} were found.");
                }
                if (builder.Paths.Length != 1)
                {
                    throw new ArgumentException($"{nameof(builder.Paths)} must contain only 1 file, but {builder.Paths.Length} were found.");
                }
                if (builder.Snippets.Any())
                {
                    throw new ArgumentException($"{nameof(builder.Snippets)} must be empty when {nameof(builder.CodeFix)} is set.");
                }
                ValidateExtension(builder.CodeFixedPath);
                if (builder.CodeFixedPathBatch is not null)
                {
                    ValidateExtension(builder.CodeFixedPathBatch);
                }
                if (codeFix.GetType().GetCustomAttribute<ExportCodeFixProviderAttribute>().Languages.Single() != language.LanguageName)
                {
                    throw new ArgumentException($"{analyzers.Single().GetType().Name} language {language.LanguageName} does not match {codeFix.GetType().Name} language.");
                }
                if (!analyzers.Single().SupportedDiagnostics.Select(x => x.Id).Intersect(codeFix.FixableDiagnosticIds).Any())
                {
                    throw new ArgumentException($"{analyzers.Single().GetType().Name} does not support diagnostics fixable by the {codeFix.GetType().Name}.");
                }
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

        public void VerifyCodeFix()     // This should never has any arguments
        {
            _ = codeFix ?? throw new InvalidOperationException($"{nameof(builder.CodeFix)} was not set.");
            var document = CreateProject(false).FindDocument(Path.GetFileName(builder.Paths.Single()));
            var codeFixVerifier = new CodeFixVerifier(analyzers.Single(), codeFix, document, builder.CodeFixTitle);
            var fixAllProvider = codeFix.GetFixAllProvider();
            foreach (var parseOptions in builder.ParseOptions.OrDefault(language.LanguageName))
            {
                codeFixVerifier.VerifyWhileDocumentChanges(parseOptions, builder.CodeFixedPath);
                if (fixAllProvider is not null)
                {
                    codeFixVerifier.VerifyFixAllProvider(fixAllProvider, parseOptions, builder.CodeFixedPathBatch ?? builder.CodeFixedPath);
                }
            }
        }

        private IEnumerable<Compilation> Compile() =>
            CreateProject(builder.ConcurrentAnalysis).Solution.Compile(builder.ParseOptions.ToArray());

        private ProjectBuilder CreateProject(bool concurrentAnalysis)
        {
            const string TestCases = "TestCases";
            using var scope = new EnvironmentVariableScope { EnableConcurrentAnalysis = concurrentAnalysis };
            var basePath = Path.GetFullPath(builder.BasePath == null ? TestCases : Path.Combine(TestCases, builder.BasePath));
            var paths = builder.Paths.Select(x => Path.Combine(basePath, x)).ToArray();
            return SolutionBuilder.Create()
                .AddProject(language, true, builder.OutputKind)
                .AddDocuments(paths)
                .AddDocuments(concurrentAnalysis && builder.AutogenerateConcurrentFiles ? CreateConcurrencyTest(paths) : Enumerable.Empty<string>())
                .AddSnippets(builder.Snippets.ToArray())
                .AddReferences(builder.References);
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

        private void ValidateExtension(string path)
        {
            if (!Path.GetExtension(path).Equals(language.FileExtension, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException($"Path '{path}' doesn't match {language.LanguageName} file extension '{language.FileExtension}'.");
            }
        }
    }
}
