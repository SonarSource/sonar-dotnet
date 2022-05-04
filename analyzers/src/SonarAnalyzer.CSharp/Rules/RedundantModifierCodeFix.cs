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

using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    public sealed class RedundantModifierCodeFix : SonarCodeFix
    {
        private const string TitleUnsafe = "Remove redundant 'unsafe' modifier";
        private const string TitleChecked = "Remove redundant 'checked' and 'unchecked' modifier";
        private const string TitlePartial = "Remove redundant 'partial' modifier";
        private const string TitleSealed = "Remove redundant 'sealed' modifier";

        private static readonly SyntaxKind[] SimpleTokenKinds =
        {
            SyntaxKind.PartialKeyword,
            SyntaxKind.SealedKeyword
        };

        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(RedundantModifier.DiagnosticId);

        protected override Task RegisterCodeFixesAsync(SyntaxNode root, CodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var token = root.FindToken(diagnosticSpan.Start);

            if (token.IsKind(SyntaxKind.UnsafeKeyword))
            {
                context.RegisterCodeFix(CodeAction.Create(TitleUnsafe, c => ReplaceRoot(context, RemoveRedundantUnsafe(root, token))), context.Diagnostics);
            }
            else if (SimpleTokenKinds.Contains(token.Kind()))
            {
                var title = token.IsKind(SyntaxKind.PartialKeyword) ? TitlePartial : TitleSealed;
                context.RegisterCodeFix(CodeAction.Create(title, c => ReplaceRoot(context, RemoveRedundantToken(root, token))), context.Diagnostics);
            }
            else if (token.Parent is CheckedStatementSyntax checkedStatement)
            {
                context.RegisterCodeFix(CodeAction.Create(TitleChecked, c => ReplaceRoot(context, RemoveRedundantCheckedStatement(root, checkedStatement))), context.Diagnostics);
            }
            else if (token.Parent is CheckedExpressionSyntax checkedExpression)
            {
                context.RegisterCodeFix(CodeAction.Create(TitleChecked, c => ReplaceRoot(context, RemoveRedundantCheckedExpression(root, checkedExpression))), context.Diagnostics);
            }
            return Task.CompletedTask;
        }

        private static Task<Document> ReplaceRoot(CodeFixContext context, SyntaxNode newRoot) =>
            Task.FromResult(context.Document.WithSyntaxRoot(newRoot));

        private static SyntaxNode RemoveRedundantUnsafe(SyntaxNode root, SyntaxToken token)
        {
            if (token.Parent is UnsafeStatementSyntax unsafeStatement)
            {
                return unsafeStatement.Parent is BlockSyntax parentBlock && parentBlock.Statements.Count == 1
                    ? root.ReplaceNode(parentBlock, parentBlock.WithStatements(unsafeStatement.Block.Statements).WithAdditionalAnnotations(Formatter.Annotation))
                    : root.ReplaceNode(unsafeStatement, unsafeStatement.Block.WithAdditionalAnnotations(Formatter.Annotation));
            }
            else
            {
                return RemoveRedundantToken(root, token);
            }
        }

        private static SyntaxNode RemoveRedundantToken(SyntaxNode root, SyntaxToken token)
        {
            var oldParent = token.Parent;
            var newParent = oldParent.ReplaceToken(token, SyntaxFactory.Token(SyntaxKind.None));
            return root.ReplaceNode(oldParent, newParent.WithLeadingTrivia(oldParent.GetLeadingTrivia()));
        }

        private static SyntaxNode RemoveRedundantCheckedStatement(SyntaxNode root, CheckedStatementSyntax checkedStatement) =>
            root.ReplaceNode(checkedStatement, SyntaxFactory.Block(checkedStatement.Block.Statements).WithTriviaFrom(checkedStatement));

        private static SyntaxNode RemoveRedundantCheckedExpression(SyntaxNode root, CheckedExpressionSyntax checkedExpression) =>
            root.ReplaceNode(checkedExpression, checkedExpression.Expression.WithTriviaFrom(checkedExpression));
    }
}
