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
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.UnitTest.TestFramework
{
    internal class CodeFixVerifier
    {
        private const string FixedMessage = "Fixed";

        private readonly DiagnosticAnalyzer analyzer;
        private readonly CodeFixProvider codeFix;
        private readonly Document originalDocument;
        private readonly string codeFixTitle;

        public CodeFixVerifier(DiagnosticAnalyzer analyzer, CodeFixProvider codeFix, Document originalDocument, string codeFixTitle)
        {
            this.analyzer = analyzer;
            this.codeFix = codeFix;
            this.originalDocument = originalDocument;
            this.codeFixTitle = codeFixTitle;
        }

        public void VerifyWhileDocumentChanges(ParseOptions parseOptions, string pathToExpected)
        {
            var state = new State(analyzer, originalDocument, parseOptions);
            var codeFixExecuted = false;
            string codeBeforeFix;
            state.Diagnostics.Should().NotBeEmpty();
            do
            {
                codeBeforeFix = state.ActualCode;
                if (state.Diagnostics
                        .Where(x => codeFix.FixableDiagnosticIds.Contains(x.Id))    // Analyzer can also raise other Diagnostics that we can't fix
                        .SelectMany(x => ActionToApply(codeFix, state.Document, x))
                        .FirstOrDefault() is { } actionToApply)
                {
                    state = new State(analyzer, ApplyCodeFix(state.Document, actionToApply), parseOptions);
                    codeFixExecuted = true;
                }
            }
            while (codeBeforeFix != state.ActualCode);

            codeFixExecuted.Should().BeTrue();
            state.AssertExpected(pathToExpected, nameof(VerifyWhileDocumentChanges) + " updates the document until all issues are fixed, even if the fix itself creates a new issue again");
        }

        public void VerifyFixAllProvider(FixAllProvider fixAllProvider, ParseOptions parseOptions, string pathToExpected)
        {
            var state = new State(analyzer, originalDocument, parseOptions);
            state.Diagnostics.Should().NotBeEmpty();

            var fixAllDiagnosticProvider = new FixAllDiagnosticProvider(state.Diagnostics);
            var fixAllContext = new FixAllContext(state.Document, codeFix, FixAllScope.Document, codeFixTitle, codeFix.FixableDiagnosticIds, fixAllDiagnosticProvider, default);
            var codeActionToExecute = fixAllProvider.GetFixAsync(fixAllContext).Result;
            codeActionToExecute.Should().NotBeNull();

            new State(analyzer, ApplyCodeFix(state.Document, codeActionToExecute), parseOptions)
                .AssertExpected(pathToExpected, $"{nameof(VerifyFixAllProvider)} runs {fixAllProvider.GetType().Name} once");
        }

        private static Document ApplyCodeFix(Document document, CodeAction codeAction)
        {
            var operations = codeAction.GetOperationsAsync(CancellationToken.None).Result;
            var solution = operations.OfType<ApplyChangesOperation>().Single().ChangedSolution;
            return solution.GetDocument(document.Id);
        }

        private IEnumerable<CodeAction> ActionToApply(CodeFixProvider codeFixProvider, Document document, Diagnostic diagnostic)
        {
            var actions = new List<CodeAction>();
            var context = new CodeFixContext(document, diagnostic, (action, _) => actions.Add(action), default);
            codeFixProvider.RegisterCodeFixesAsync(context).Wait();
            return actions.Where(x => codeFixTitle is null || x.Title == codeFixTitle);
        }

        private class State
        {
            public readonly Document Document;
            public readonly ImmutableArray<Diagnostic> Diagnostics;
            public readonly string ActualCode;
            private readonly Compilation compilation;

            public State(DiagnosticAnalyzer analyzer, Document document, ParseOptions parseOptions)
            {
                var project = document.Project;
                if (parseOptions != null)
                {
                    project = project.WithParseOptions(parseOptions);
                    document = project.GetDocument(document.Id);    // There's a new instance with the same ID
                }

                compilation = project.GetCompilationAsync().Result;
                Document = document;
                Diagnostics = DiagnosticVerifier.GetDiagnosticsNoExceptions(compilation, analyzer, CompilationErrorBehavior.Ignore).ToImmutableArray();
                ActualCode = document.GetSyntaxRootAsync().Result.GetText().ToString();
            }

            public void AssertExpected(string pathToExpected, string becauseMessage)
            {
                var expected = File.ReadAllText(pathToExpected).ToUnixLineEndings();
                ActualCodeWithReplacedComments().ToUnixLineEndings().Should().Be(expected, $"{becauseMessage}. Language: {compilation.LanguageVersionString()}");
            }

            private string ActualCodeWithReplacedComments() =>
                ActualCode.ToUnixLineEndings()
                    .Split(new[] { Constants.UnixLineEnding }, StringSplitOptions.None)
                    .Where(x => !IssueLocationCollector.RxPreciseLocation.IsMatch(x))
                    .Select(ReplaceNonCompliantComment)
                    .JoinStr(Constants.UnixLineEnding);
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
