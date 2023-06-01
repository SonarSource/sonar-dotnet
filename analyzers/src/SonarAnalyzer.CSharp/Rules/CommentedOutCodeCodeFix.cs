/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Trivias = System.Collections.Generic.Dictionary<Microsoft.CodeAnalysis.FileLinePositionSpan, Microsoft.CodeAnalysis.SyntaxTrivia>;

namespace SonarAnalyzer.Rules.CSharp;

[ExportCodeFixProvider(LanguageNames.CSharp)]
public sealed class CommentedOutCodeCodeFix : SonarCodeFix
{
    public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(CommentedOutCode.DiagnosticId);

    public override FixAllProvider GetFixAllProvider() => null;

    protected override Task RegisterCodeFixesAsync(SyntaxNode root, CodeFixContext context)
    {
        var diagnostic = context.Diagnostics.First();

        context.RegisterCodeFix(CodeAction.Create(
            CommentedOutCode.MessageFormat,
            c =>
            {
                var comment = diagnostic.Location.GetLineSpan();
                var node = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);

                var newNode = node;

                if (Remove(node.GetTrailingTrivia(), comment) is { } trailing)
                {
                    var token = node.ChildTokens().Last();
                    ClearEmptiedLines(trailing, token, comment);
                    RemoveWhitespace(trailing, token);
                    newNode = node.WithTrailingTrivia(trailing.Values);
                }
                else if (Remove(node.GetLeadingTrivia(), comment) is { } leading)
                {
                    ClearEmptiedLines(leading, node.ChildTokens().First(), comment);
                    newNode = node.WithLeadingTrivia(leading.Values);
                }
                else
                {
                    // this should never occur.
                }

                return Task.FromResult(context.Document.WithSyntaxRoot(root.ReplaceNode(node, newNode)));
            }),
            diagnostic);

        return Task.CompletedTask;
    }

    private static Trivias Remove(SyntaxTriviaList trivias, FileLinePositionSpan comment)
    {
        var spans = trivias.ToDictionary(x => x.GetLocation().GetLineSpan(), x => x);
        return spans.Remove(comment) ? spans : null;
    }

    private static void ClearEmptiedLines(Trivias triviaList, SyntaxToken token, FileLinePositionSpan comment)
    {
        if (!ShareLine(comment, token))
        {
            foreach (var shared in triviaList.Keys.Where(x => ShareLine(comment, x)).ToArray())
            {
                triviaList.Remove(shared);
            }
        }
    }

    private static void RemoveWhitespace(Trivias triviaList, SyntaxToken token)
    {
        foreach (var whitespace in triviaList
            .Where(x => x.Value.IsKind(SyntaxKind.WhitespaceTrivia) && ShareLine(x.Key, token))
            .Select(x => x.Key)
            .ToArray())
        {
            triviaList.Remove(whitespace);
        }
    }

    private static bool ShareLine(FileLinePositionSpan span, FileLinePositionSpan trivia) =>
        span.GetLineNumbers().Intersect(trivia.GetLineNumbers()).Any();

    private static bool ShareLine(FileLinePositionSpan span, SyntaxToken token) =>
        span.GetLineNumbers().Intersect(token.GetLineNumbers()).Any();
}
