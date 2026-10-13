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

using Microsoft.CodeAnalysis.Text;

namespace SonarAnalyzer.CSharp.Rules;

[ExportCodeFixProvider(LanguageNames.CSharp)]
public sealed class RedundantParenthesesCodeFix : SonarCodeFix
{
    internal const string Title = "Remove redundant parentheses";

    public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(RedundantParentheses.DiagnosticId, RedundantParenthesesObjectsCreation.DiagnosticId);

    protected override Task RegisterCodeFixesAsync(SyntaxNode root, SonarCodeFixContext context)
    {
        var diagnostic = context.Diagnostics.First();
        var diagnosticSpan = diagnostic.Location.SourceSpan;
        var node = root.FindNode(diagnosticSpan, getInnermostNodeForTie: true);

        if (node is ArgumentListSyntax or AttributeArgumentListSyntax)
        {
            context.RegisterCodeFix(
                Title,
                x =>
                {
                    var newRoot = root.RemoveNode(node, SyntaxRemoveOptions.KeepExteriorTrivia | SyntaxRemoveOptions.KeepEndOfLine);
                    return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
                },
                context.Diagnostics);
        }
        else if (node is ParenthesizedExpressionSyntax { Expression: ParenthesizedExpressionSyntax } expression
            && !expression.ContainsDiagnostics
            && !expression.ContainsDirectives
            && RemoveRedundantParentheses(expression) is var fixedExpression
            && ReparsesUnchanged(root, expression, fixedExpression))
        {
            context.RegisterCodeFix(
                Title,
                x => Task.FromResult(context.Document.WithSyntaxRoot(root.ReplaceNode(expression, fixedExpression))),
                context.Diagnostics);
        }

        return Task.CompletedTask;
    }

    private static ParenthesizedExpressionSyntax RemoveRedundantParentheses(ParenthesizedExpressionSyntax expression)
    {
        while (expression.Expression is ParenthesizedExpressionSyntax inner)
        {
            var openParen = expression.OpenParenToken;
            var closeParen = expression.CloseParenToken;
            var beforeCloseParen = inner.GetTrailingTrivia().AddRange(closeParen.LeadingTrivia);
            var afterCloseParen = closeParen.TrailingTrivia;
            if (beforeCloseParen.Any(SyntaxKind.EndOfLineTrivia))
            {
                afterCloseParen = WithoutLeadingWhitespace(afterCloseParen);
            }
            else
            {
                beforeCloseParen = WithoutTrailingWhitespace(beforeCloseParen);
            }
            expression = inner
                .WithLeadingTrivia(openParen.LeadingTrivia.AddRange(WithoutLeadingWhitespace(openParen.TrailingTrivia)).AddRange(inner.GetLeadingTrivia()))
                .WithTrailingTrivia(beforeCloseParen.AddRange(afterCloseParen));
        }
        return expression;
    }

    private static bool ReparsesUnchanged(SyntaxNode root, ParenthesizedExpressionSyntax expression, ParenthesizedExpressionSyntax fixedExpression)
    {
        var tree = root.SyntaxTree;
        var fixedText = tree.GetText().WithChanges(new TextChange(expression.FullSpan, fixedExpression.ToFullString()));
        return SyntaxFactory.AreEquivalent(tree.WithChangedText(fixedText).GetRoot(), root.ReplaceNode(expression, fixedExpression), topLevel: false);
    }

    private static SyntaxTriviaList WithoutLeadingWhitespace(SyntaxTriviaList trivia) =>
        trivia.FirstOrDefault().IsKind(SyntaxKind.WhitespaceTrivia) ? trivia.RemoveAt(0) : trivia;

    private static SyntaxTriviaList WithoutTrailingWhitespace(SyntaxTriviaList trivia) =>
        trivia.LastOrDefault().IsKind(SyntaxKind.WhitespaceTrivia) ? trivia.RemoveAt(trivia.Count - 1) : trivia;
}
