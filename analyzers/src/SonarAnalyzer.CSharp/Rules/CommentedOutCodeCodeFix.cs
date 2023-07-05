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

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;

namespace SonarAnalyzer.Rules.CSharp;

[ExportCodeFixProvider(LanguageNames.CSharp)]
public sealed class CommentedOutCodeCodeFix : SonarCodeFix
{
    public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(CommentedOutCode.DiagnosticId);

    public override FixAllProvider GetFixAllProvider() => null;

    protected override Task RegisterCodeFixesAsync(SyntaxNode root, CodeFixContext context)
    {
        var diagnostic = context.Diagnostics.First();
        var comment = root.FindTrivia(diagnostic.Location.SourceSpan.Start, findInsideTrivia: true);

        if (comment.IsKind(SyntaxKind.SingleLineCommentTrivia) || IsCode(comment))
        {
            context.RegisterCodeFix(CodeAction.Create(
                CommentedOutCode.MessageFormat,
                c => new Context(comment).ChangeDocument(context.Document, root)),
                diagnostic);
        }
        return Task.CompletedTask;
    }

    private static bool IsCode(SyntaxTrivia trivia) =>
        trivia.GetLineNumbers().Count() == 1
        || Array.TrueForAll(Lines(trivia), IsCode);

    private static bool IsCode(string line)
    {
        var trimmed = line.Trim();
        return trimmed == "/*"
            || trimmed == "*/"
            || trimmed == "*"
            || string.IsNullOrEmpty(trimmed)
            || CommentedOutCode.IsCode(line);
    }

    private static string[] Lines(SyntaxTrivia trivia) =>
        trivia.ToFullString().Split(MetricsBase.LineTerminators, StringSplitOptions.None);

    internal sealed class Context
    {
        public Context(SyntaxTrivia comment)
        {
            Comment = comment;
            Leading = Token.LeadingTrivia.ToList();
            Trailing = Token.TrailingTrivia.ToList();

            IsTrailing = Trailing.Remove(comment);

            if (!IsTrailing)
            {
                Leading.Remove(comment);
            }
        }

        public bool IsTrailing { get; }
        public SyntaxTrivia Comment { get; }
        public List<SyntaxTrivia> Leading { get; }
        public List<SyntaxTrivia> Trailing { get; }
        public SyntaxToken Token => Comment.Token;

        public Task<Document> ChangeDocument(Document document, SyntaxNode root) =>
            Task.FromResult(document.WithSyntaxRoot(root.ReplaceToken(Token, NewToken)));

        public SyntaxToken NewToken =>
            IsTrailing ? NewTrailing() : NewLeading();

        private SyntaxToken NewTrailing()
        {
            var trailing = ShareLine(Token, Comment)
                ? Trailing.Where(t
                    => !ShareLine(t, Comment)
                    || (t.IsKind(SyntaxKind.EndOfLineTrivia) && ShareLine(t, Comment))).ToList()
                : Trailing;

            // this can happen for multi line comments within an expression.
            if (trailing.Count == 0 && ShareLine(Token.GetNextToken(), Comment))
            {
                trailing.Add(SyntaxFactory.Space);
            }

            return Token
                .WithLeadingTrivia(Leading)
                .WithTrailingTrivia(trailing);
        }
        private SyntaxToken NewLeading() => Token
            .WithLeadingTrivia(Leading.Where(t => !ShareLine(t, Comment)))
            .WithTrailingTrivia(Trailing);

        private static bool ShareLine(SyntaxToken token, SyntaxTrivia trivia) =>
            ShareLine(token.GetLineNumbers(), trivia.GetLineNumbers());

        private static bool ShareLine(SyntaxTrivia l, SyntaxTrivia r) =>
            ShareLine(l.GetLineNumbers(), r.GetLineNumbers());

        private static bool ShareLine(IEnumerable<int> l, IEnumerable<int> r) =>
            l.Intersect(r).Any();
    }
}
