/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2020 SonarSource SA
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

using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.UnitTest.TestFramework
{
    internal static class CodeFixVerifier
    {
        public static void VerifyCodeFix(string path, string pathToExpected, string pathToBatchExpected,
            SonarDiagnosticAnalyzer diagnosticAnalyzer, SonarCodeFixProvider codeFixProvider, string codeFixTitle,
            IEnumerable<ParseOptions> options = null, IEnumerable<MetadataReference> additionalReferences = null)
        {
            var document = CreateDocument(path, additionalReferences);
            var parseOptions = ParseOptionsHelper.GetParseOptionsOrDefault(options)
                .Where(ParseOptionsHelper.GetFilterByLanguage(document.Project.Language))
                .ToArray();

            foreach (var parseOption in parseOptions)
            {
                RunCodeFixWhileDocumentChanges(diagnosticAnalyzer, codeFixProvider, codeFixTitle, document, parseOption, pathToExpected);
            }

            var fixAllProvider = codeFixProvider.GetFixAllProvider();
            if (fixAllProvider == null)
            {
                return;
            }

            foreach (var parseOption in parseOptions)
            {
                RunFixAllProvider(diagnosticAnalyzer, codeFixProvider, codeFixTitle, fixAllProvider, document, parseOption, pathToBatchExpected);
            }
        }

        private static Document CreateDocument(string path, IEnumerable<MetadataReference> additionalReferences) =>
            SolutionBuilder.Create()
                .AddProject(AnalyzerLanguage.FromPath(path))
                .AddReferences(additionalReferences)
                .AddDocument(path, true)
                .FindDocument(Path.GetFileName(path));

        private static void RunCodeFixWhileDocumentChanges(DiagnosticAnalyzer diagnosticAnalyzer, CodeFixProvider codeFixProvider,
            string codeFixTitle, Document document, ParseOptions parseOption, string pathToExpected)
        {
            var currentDocument = document;
            var state = new DocumentState(diagnosticAnalyzer, currentDocument, parseOption);
            state.Diagnostics.Should().NotBeEmpty();

            string codeBeforeFix;
            var codeFixExecutedAtLeastOnce = false;
            do
            {
                codeBeforeFix = state.ActualCode;

                var codeFixExecuted = false;
                for (var diagnosticIndexToFix = 0; !codeFixExecuted && diagnosticIndexToFix < state.Diagnostics.Length; diagnosticIndexToFix++)
                {
                    var diagnostic = state.Diagnostics[diagnosticIndexToFix];
                    if (!codeFixProvider.FixableDiagnosticIds.Contains(diagnostic.Id))
                    {
                        // a Diagnostic can raise issues for different rules, but provide fixes for only one of them
                        // if we don't have a fixer for this rule, we skip it
                        continue;
                    }
                    var codeActionsForDiagnostic = GetCodeActionsForDiagnostic(codeFixProvider, currentDocument, diagnostic);

                    if (CodeActionToApply(codeFixTitle, codeActionsForDiagnostic) is { } codeActionToApply)
                    {
                        currentDocument = ApplyCodeFix(currentDocument, codeActionToApply);
                        state = new DocumentState(diagnosticAnalyzer, currentDocument, parseOption);

                        codeFixExecutedAtLeastOnce = true;
                        codeFixExecuted = true;
                    }
                }
            } while (codeBeforeFix != state.ActualCode);

            codeFixExecutedAtLeastOnce.Should().BeTrue();

            AreEqualIgnoringLineEnding(pathToExpected, state);
        }

        private static void AreEqualIgnoringLineEnding(string pathToExpected, DocumentState state)
        {
            const string WindowsLineEnding = "\r\n";
            const string UnixLineEnding = "\n";

            var actualWithUnixLineEnding = state.ActualCode.Replace(WindowsLineEnding, UnixLineEnding);
            var expectedWithUnixLineEnding = File.ReadAllText(pathToExpected).Replace(WindowsLineEnding, UnixLineEnding);
            actualWithUnixLineEnding.Should().Be(expectedWithUnixLineEnding, state.Compilation.LanguageVersionString());
        }

        private static void RunFixAllProvider(DiagnosticAnalyzer diagnosticAnalyzer, CodeFixProvider codeFixProvider,
            string codeFixTitle, FixAllProvider fixAllProvider, Document document, ParseOptions parseOption, string pathToExpected)
        {
            var currentDocument = document;
            var state = new DocumentState(diagnosticAnalyzer, currentDocument, parseOption);

            state.Diagnostics.Should().NotBeEmpty();

            var fixAllDiagnosticProvider = new FixAllDiagnosticProvider(
                codeFixProvider.FixableDiagnosticIds.ToHashSet(),
                (doc, ids, ct) => Task.FromResult(state.Diagnostics.AsEnumerable()),
                null);

            var fixAllContext = new FixAllContext(currentDocument, codeFixProvider, FixAllScope.Document,
                codeFixTitle, codeFixProvider.FixableDiagnosticIds, fixAllDiagnosticProvider, CancellationToken.None);
            var codeActionToExecute = fixAllProvider.GetFixAsync(fixAllContext).Result;

            codeActionToExecute.Should().NotBeNull();

            currentDocument = ApplyCodeFix(currentDocument, codeActionToExecute);
            state = new DocumentState(diagnosticAnalyzer, currentDocument, parseOption);
            AreEqualIgnoringLineEnding(pathToExpected, state);
        }

        private static Document ApplyCodeFix(Document document, CodeAction codeAction)
        {
            var operations = codeAction.GetOperationsAsync(CancellationToken.None).Result;
            var solution = operations.OfType<ApplyChangesOperation>().Single().ChangedSolution;
            return solution.GetDocument(document.Id);
        }

        private static CodeAction CodeActionToApply(string codeFixTitle, IEnumerable<CodeAction> codeActions) =>
            codeFixTitle == null
                ? codeActions.FirstOrDefault()
                : codeActions.SingleOrDefault(action => action.Title == codeFixTitle);

        private static IEnumerable<CodeAction> GetCodeActionsForDiagnostic(CodeFixProvider codeFixProvider, Document document, Diagnostic diagnostic)
        {
            var actions = new List<CodeAction>();
            var context = new CodeFixContext(document, diagnostic, (a, d) => actions.Add(a), CancellationToken.None);

            codeFixProvider.RegisterCodeFixesAsync(context).Wait();
            return actions;
        }

        private class DocumentState
        {
            public readonly Compilation Compilation;
            public readonly ImmutableArray<Diagnostic> Diagnostics;
            public readonly string ActualCode;

            public DocumentState(DiagnosticAnalyzer diagnosticAnalyzer, Document document, ParseOptions parseOption)
            {
                var project = document.Project;
                if (parseOption != null)
                {
                    project = project.WithParseOptions(parseOption);
                }

                Compilation = project.GetCompilationAsync().Result;
                Diagnostics = DiagnosticVerifier.GetDiagnostics(Compilation, diagnosticAnalyzer, CompilationErrorBehavior.Ignore).ToImmutableArray();
                ActualCode = document.GetSyntaxRootAsync().Result.GetText().ToString();
            }

        }
    }
}
