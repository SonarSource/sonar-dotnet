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
using Microsoft.CodeAnalysis.Text;

namespace SonarAnalyzer.Rules.CSharp;

[ExportCodeFixProvider(LanguageNames.CSharp)]
public sealed class CommentedOutCodeCodeFix : SonarCodeFix
{
    public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(CommentedOutCode.DiagnosticId);

    protected override Task RegisterCodeFixesAsync(SyntaxNode root, CodeFixContext context)
    {
        var diagnostic = context.Diagnostics.First();

        context.RegisterCodeFix(CodeAction.Create(
            CommentedOutCode.MessageFormat,
            c =>
            {
                var comment = diagnostic.Location.SourceSpan;
                var node = root.FindNode(comment);
                var trivia = node.GetLeadingTrivia();
                var newNode = node.WithLeadingTrivia(Remaining(trivia, comment));
                return Task.FromResult(context.Document.WithSyntaxRoot(root.ReplaceNode(node, newNode)));
            }),
            diagnostic);

        return Task.CompletedTask;
    }

    private IEnumerable<SyntaxTrivia> Remaining(SyntaxTriviaList trivia, TextSpan remove) =>
        trivia.Where(t => t.FullSpan.Start < remove.Start || t.FullSpan.End > remove.End);
}
