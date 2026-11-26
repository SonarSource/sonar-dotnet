/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using SonarAnalyzer.TestFramework.Verification.IssueValidation;

namespace SonarAnalyzer.TestFramework.Verification;

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

    public void VerifyWhileDocumentChanges(ParseOptions parseOptions, string pathToExpected) =>
        Task.Run(() => VerifyWhileDocumentChangesAsync(parseOptions, pathToExpected)).GetAwaiter().GetResult();

    public void VerifyFixAllProvider(FixAllProvider fixAllProvider, ParseOptions parseOptions, string pathToExpected) =>
        Task.Run(() => VerifyFixAllProviderAsync(fixAllProvider, parseOptions, pathToExpected)).GetAwaiter().GetResult();

    private async Task VerifyWhileDocumentChangesAsync(ParseOptions parseOptions, string pathToExpected)
    {
        var state = await State.CreateAsync(analyzer, originalDocument, parseOptions);
        var codeFixExecuted = false;
        string codeBeforeFix;
        state.Diagnostics.Should().NotBeEmpty();
        do
        {
            codeBeforeFix = state.ActualCode;
            var fixableDiagnostics = state.Diagnostics.Where(x => codeFix.FixableDiagnosticIds.Contains(x.Id));    // Analyzer can also raise other Diagnostics that we can't fix
            if (await FirstActionToApply(codeFix, state.Document, fixableDiagnostics) is { } actionToApply)
            {
                state = await State.CreateAsync(analyzer, await ApplyCodeFix(state.Document, actionToApply), parseOptions);
                codeFixExecuted = true;
            }
        }
        while (codeBeforeFix != state.ActualCode);

        codeFixExecuted.Should().BeTrue();
        state.AssertExpected(pathToExpected, nameof(VerifyWhileDocumentChanges) + " updates the document until all issues are fixed, even if the fix itself creates a new issue again");
    }

    private async Task VerifyFixAllProviderAsync(FixAllProvider fixAllProvider, ParseOptions parseOptions, string pathToExpected)
    {
        var state = await State.CreateAsync(analyzer, originalDocument, parseOptions);
        state.Diagnostics.Should().NotBeEmpty();

        var fixAllDiagnosticProvider = new FixAllDiagnosticProvider(state.Diagnostics);
        var codeActionEquivalenceKey = codeFixTitle ?? await CodeFixTitle(codeFix, state);    // We need to find the title of the single action to use
        var fixAllContext = new FixAllContext(state.Document, codeFix, FixAllScope.Document, codeActionEquivalenceKey, codeFix.FixableDiagnosticIds, fixAllDiagnosticProvider, default);
        var codeActionToExecute = await fixAllProvider.GetFixAsync(fixAllContext);
        codeActionToExecute.Should().NotBeNull();

        (await State.CreateAsync(analyzer, await ApplyCodeFix(state.Document, codeActionToExecute), parseOptions))
            .AssertExpected(pathToExpected, $"{nameof(VerifyFixAllProvider)} runs {fixAllProvider.GetType().Name} once");
    }

    private async Task<string> CodeFixTitle(CodeFixProvider codeFix, State state) =>
        (await FirstActionToApply(codeFix, state.Document, state.Diagnostics) ?? throw new InvalidOperationException("No code action was registered for the diagnostics.")).Title;

    private static async Task<Document> ApplyCodeFix(Document document, CodeAction codeAction)
    {
        var operations = await codeAction.GetOperationsAsync(CancellationToken.None);
        var solution = operations.OfType<ApplyChangesOperation>().Single().ChangedSolution;
        return solution.GetDocument(document.Id);
    }

    private async Task<CodeAction> FirstActionToApply(CodeFixProvider codeFix, Document document, IEnumerable<Diagnostic> diagnostics)
    {
        foreach (var diagnostic in diagnostics)
        {
            if ((await ActionToApply(codeFix, document, diagnostic)).FirstOrDefault() is { } action)
            {
                return action;
            }
        }
        return null;
    }

    private async Task<IEnumerable<CodeAction>> ActionToApply(CodeFixProvider codeFix, Document document, Diagnostic diagnostic)
    {
        var actions = new List<CodeAction>();
        var context = new CodeFixContext(document, diagnostic, (action, _) => actions.Add(action), default);
        await codeFix.RegisterCodeFixesAsync(context);
        return actions.Where(x => codeFixTitle is null || x.Title == codeFixTitle);
    }

    private sealed class State
    {
        public readonly Document Document;
        public readonly ImmutableArray<Diagnostic> Diagnostics;
        public readonly string ActualCode;
        private readonly Compilation compilation;

        private State(Document document, ImmutableArray<Diagnostic> diagnostics, string actualCode, Compilation compilation)
        {
            Document = document;
            Diagnostics = diagnostics;
            ActualCode = actualCode;
            this.compilation = compilation;
        }

        public static async Task<State> CreateAsync(DiagnosticAnalyzer analyzer, Document document, ParseOptions parseOptions)
        {
            var project = document.Project.WithParseOptions(parseOptions);
            document = project.GetDocument(document.Id);    // There's a new instance with the same ID
            var compilation = await project.GetCompilationAsync();
            var diagnostics = DiagnosticVerifier.AnalyzerDiagnostics(compilation, analyzer, default).Where(x => x.Severity != DiagnosticSeverity.Error).ToImmutableArray();
            var root = await document.GetSyntaxRootAsync();
            return new State(document, diagnostics, root.GetText().ToString(), compilation);
        }

        public void AssertExpected(string pathToExpected, string becauseMessage)
        {
            var expected = File.ReadAllText(pathToExpected).ToUnixLineEndings();
            var actual = ActualCodeWithReplacedComments().ToUnixLineEndings();
            actual.Should().Be(expected, $"{becauseMessage}. Language: {compilation.LanguageVersionString()}");
        }

        private string ActualCodeWithReplacedComments() =>
            ActualCode.ToUnixLineEndings()
                .Split(new[] { TestConstants.UnixLineEnding }, StringSplitOptions.None)
                .Where(x => !IssueLocationCollector.RxPreciseLocation.IsMatch(x))
                .Select(ReplaceNonCompliantComment)
                .JoinStr(TestConstants.UnixLineEnding);

        private static string ReplaceNonCompliantComment(string line)
        {
            var match = IssueLocationCollector.RxIssue.Match(line);
            if (!match.Success || match.Groups["IssueType"].Value == "Error")
            {
                return line;
            }

            if (match.Groups["IssueType"].Value == "Noncompliant")
            {
                var startIndex = line.IndexOf(match.Groups["IssueType"].Value);
                return string.Concat(line.Remove(startIndex), FixedMessage);
            }

            return line.Replace(match.Value, string.Empty).TrimEnd();
        }
    }
}
