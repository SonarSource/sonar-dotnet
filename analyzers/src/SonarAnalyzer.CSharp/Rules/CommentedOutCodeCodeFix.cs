﻿/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

using Microsoft.CodeAnalysis.CodeFixes;

namespace SonarAnalyzer.Rules.CSharp;

[ExportCodeFixProvider(LanguageNames.CSharp)]
public sealed class CommentedOutCodeCodeFix : SonarCodeFix
{
    public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(CommentedOutCode.DiagnosticId);

    public override FixAllProvider GetFixAllProvider() => null;

    protected override Task RegisterCodeFixesAsync(SyntaxNode root, SonarCodeFixContext context)
    {
        var diagnostic = context.Diagnostics.First();
        var comment = root.FindTrivia(diagnostic.Location.SourceSpan.Start, findInsideTrivia: true);

        if (comment.IsKind(SyntaxKind.SingleLineCommentTrivia) || IsCode(comment))
        {
            context.RegisterCodeFix(
                CommentedOutCode.MessageFormat,
                c => new Context(comment).ChangeDocument(context.Document, root),
                context.Diagnostics);
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

    private sealed class Context
    {
        private bool IsTrailing { get; }
        private SyntaxTrivia Comment { get; }
        private List<SyntaxTrivia> Leading { get; }
        private List<SyntaxTrivia> Trailing { get; }
        private SyntaxToken Token => Comment.Token;

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

        public Task<Document> ChangeDocument(Document document, SyntaxNode root) =>
            Task.FromResult(document.WithSyntaxRoot(root.ReplaceToken(Token, NewToken())));

        private SyntaxToken NewToken() =>
            IsTrailing ? NewTrailing() : NewLeading();

        private SyntaxToken NewTrailing()
        {
            var trailing = ShareLine(Token, Comment)
                 ? Trailing.Where(KeepTrailing).ToList()
                 : Trailing;

            // this can happen for multi line comments within an expression. Like:
            // int /* commented code */ MethodCall()
            if (trailing.Count == 0 && ShareLine(Token.GetNextToken(), Comment))
            {
                trailing.Add(SyntaxFactory.Space);
            }

            return Token
                .WithLeadingTrivia(Leading)
                .WithTrailingTrivia(trailing);
        }

        private bool KeepTrailing(SyntaxTrivia trivia) =>
             trivia.IsKind(SyntaxKind.EndOfLineTrivia) || !ShareLine(trivia, Comment);

        private SyntaxToken NewLeading() =>
            Token
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
