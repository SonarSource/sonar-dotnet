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

using System.Collections.Generic;
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
            var parseOptions = options ?? ParseOptionsHelper.GetParseOptionsByFileExtension(Path.GetExtension(path));

            foreach (var parseOption in parseOptions)
            {
                RunCodeFixWhileDocumentChanges(diagnosticAnalyzer, codeFixProvider, codeFixTitle,
                    CreateDocument(path, additionalReferences), parseOption, pathToExpected);
            }

            var fixAllProvider = codeFixProvider.GetFixAllProvider();
            if (fixAllProvider == null)
            {
                return;
            }

            foreach (var parseOption in parseOptions)
            {
                RunFixAllProvider(diagnosticAnalyzer, codeFixProvider, codeFixTitle, fixAllProvider,
                    CreateDocument(path, additionalReferences), parseOption, pathToBatchExpected);
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
            CalculateDiagnosticsAndCode(diagnosticAnalyzer, currentDocument, parseOption, out var diagnostics, out var actualCode);

            diagnostics.Should().NotBeEmpty();

            string codeBeforeFix;
            var codeFixExecutedAtLeastOnce = false;

            do
            {
                codeBeforeFix = actualCode;

                var codeFixExecuted = false;
                for (var diagnosticIndexToFix = 0; !codeFixExecuted && diagnosticIndexToFix < diagnostics.Count; diagnosticIndexToFix++)
                {
                    var codeActionsForDiagnostic = GetCodeActionsForDiagnostic(codeFixProvider, currentDocument, diagnostics[diagnosticIndexToFix]);

                    if (TryGetCodeActionToApply(codeFixTitle, codeActionsForDiagnostic, out var codeActionToExecute))
                    {
                        currentDocument = ApplyCodeFix(currentDocument, codeActionToExecute);
                        CalculateDiagnosticsAndCode(diagnosticAnalyzer, currentDocument, parseOption, out diagnostics, out actualCode);

                        codeFixExecutedAtLeastOnce = true;
                        codeFixExecuted = true;
                    }
                }
            } while (codeBeforeFix != actualCode);

            codeFixExecutedAtLeastOnce.Should().BeTrue();

            AreEqualIgnoringLineEnding(pathToExpected, actualCode);
        }

        private static void AreEqualIgnoringLineEnding(string pathToExpected, string actualCode)
        {
            const string WindowsLineEnding = "\r\n";
            const string UnixLineEnding = "\n";

            var actualWithUnixLineEnding = actualCode.Replace(WindowsLineEnding, UnixLineEnding);
            var expectedWithUnixLineEnding = File.ReadAllText(pathToExpected).Replace(WindowsLineEnding, UnixLineEnding);
            actualWithUnixLineEnding.Should().Be(expectedWithUnixLineEnding);
        }

        private static void RunFixAllProvider(DiagnosticAnalyzer diagnosticAnalyzer, CodeFixProvider codeFixProvider,
            string codeFixTitle, FixAllProvider fixAllProvider, Document document, ParseOptions parseOption, string pathToExpected)
        {
            var currentDocument = document;
            CalculateDiagnosticsAndCode(diagnosticAnalyzer, currentDocument, parseOption, out var diagnostics, out var actualCode);

            diagnostics.Should().NotBeEmpty();

            var fixAllDiagnosticProvider = new FixAllDiagnosticProvider(
                codeFixProvider.FixableDiagnosticIds.ToHashSet(),
                (doc, ids, ct) => Task.FromResult(
                    DiagnosticVerifier.GetDiagnostics(
                        currentDocument.Project.GetCompilationAsync(ct).Result,
                        diagnosticAnalyzer, CompilationErrorBehavior.Ignore)), // TODO: Is that the right decision?
                null);

            var fixAllContext = new FixAllContext(currentDocument, codeFixProvider, FixAllScope.Document,
                codeFixTitle, codeFixProvider.FixableDiagnosticIds, fixAllDiagnosticProvider, CancellationToken.None);
            var codeActionToExecute = fixAllProvider.GetFixAsync(fixAllContext).Result;

            codeActionToExecute.Should().NotBeNull();

            currentDocument = ApplyCodeFix(currentDocument, codeActionToExecute);

            CalculateDiagnosticsAndCode(diagnosticAnalyzer, currentDocument, parseOption, out diagnostics, out actualCode);

            AreEqualIgnoringLineEnding(pathToExpected, actualCode);
        }

        private static void CalculateDiagnosticsAndCode(DiagnosticAnalyzer diagnosticAnalyzer, Document document, ParseOptions parseOption,
            out List<Diagnostic> diagnostics,
            out string actualCode)
        {
            var project = document.Project;
            if (parseOption != null)
            {
                project = project.WithParseOptions(parseOption);
            }

            diagnostics = DiagnosticVerifier.GetDiagnostics(project.GetCompilationAsync().Result,
                diagnosticAnalyzer, CompilationErrorBehavior.Ignore).ToList(); // TODO: Is that the right choice?
            actualCode = document.GetSyntaxRootAsync().Result.GetText().ToString();
        }

        private static Document ApplyCodeFix(Document document, CodeAction codeAction)
        {
            var operations = codeAction.GetOperationsAsync(CancellationToken.None).Result;
            var solution = operations.OfType<ApplyChangesOperation>().Single().ChangedSolution;
            return solution.GetDocument(document.Id);
        }

        private static bool TryGetCodeActionToApply(string codeFixTitle, IEnumerable<CodeAction> codeActions,
            out CodeAction codeAction)
        {
            codeAction = codeFixTitle != null
                ? codeActions.SingleOrDefault(action => action.Title == codeFixTitle)
                : codeActions.FirstOrDefault();

            return codeAction != null;
        }

        private static IEnumerable<CodeAction> GetCodeActionsForDiagnostic(CodeFixProvider codeFixProvider, Document document,
            Diagnostic diagnostic)
        {
            var actions = new List<CodeAction>();
            var context = new CodeFixContext(document, diagnostic, (a, d) => actions.Add(a), CancellationToken.None);

            codeFixProvider.RegisterCodeFixesAsync(context).Wait();
            return actions;
        }
    }
}
