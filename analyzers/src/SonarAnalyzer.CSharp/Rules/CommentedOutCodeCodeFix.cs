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
                var node = root.FindNode(diagnostic.Location.SourceSpan, getInnermostNodeForTie: true);
                var trailing = node.GetTrailingTrivia();
                var remaining = trailing.Where(t => t.Span.Start != diagnostic.Location.SourceSpan.Start);
                var newNode = node.WithTrailingTrivia(remaining);
                return Task.FromResult(context.Document.WithSyntaxRoot(root.ReplaceNode(node, newNode)));
            }),
            diagnostic);

        return Task.CompletedTask;
    }
}
