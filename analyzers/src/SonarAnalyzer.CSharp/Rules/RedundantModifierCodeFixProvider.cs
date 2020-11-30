/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2020 SonarSource SA
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
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    public sealed class RedundantModifierCodeFixProvider : SonarCodeFixProvider
    {
        internal const string TitleUnsafe = "Remove redundant 'unsafe' modifier";
        internal const string TitleChecked = "Remove redundant 'checked' and 'unchecked' modifier";
        internal const string TitlePartial = "Remove redundant 'partial' modifier";
        internal const string TitleSealed = "Remove redundant 'sealed' modifier";

        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(RedundantModifier.DiagnosticId);
            }
        }

        public override FixAllProvider GetFixAllProvider()
        {
            return DocumentBasedFixAllProvider.Instance;
        }

        private static readonly ISet<SyntaxKind> SimpleTokenKinds = new HashSet<SyntaxKind>
        {
            SyntaxKind.PartialKeyword,
            SyntaxKind.SealedKeyword
        };

        protected override Task RegisterCodeFixesAsync(SyntaxNode root, CodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var token = root.FindToken(diagnosticSpan.Start);

            if (token.IsKind(SyntaxKind.UnsafeKeyword))
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        TitleUnsafe,
                        c =>
                        {
                            var newRoot = RemoveRedundantUnsafe(root, token);
                            return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
                        }),
                    context.Diagnostics);

                return TaskHelper.CompletedTask;
            }

            if (SimpleTokenKinds.Contains(token.Kind()))
            {
                var title = token.IsKind(SyntaxKind.PartialKeyword) ? TitlePartial : TitleSealed;

                context.RegisterCodeFix(
                    CodeAction.Create(
                        title,
                        c =>
                        {
                            var newRoot = RemoveRedundantToken(root, token);
                            return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
                        }),
                    context.Diagnostics);

                return TaskHelper.CompletedTask;
            }

            RegisterCodeFixForChecked(token.Parent, root, context);

            return TaskHelper.CompletedTask;
        }

        private static void RegisterCodeFixForChecked(SyntaxNode node, SyntaxNode root, CodeFixContext context)
        {
            if (node is CheckedStatementSyntax checkedStatement)
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        TitleChecked,
                        c =>
                        {
                            var newRoot = RemoveRedundantCheckedStatement(root, checkedStatement);
                            return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
                        }),
                    context.Diagnostics);
                return;
            }

            if (node is CheckedExpressionSyntax checkedExpression)
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        TitleChecked,
                        c =>
                        {
                            var newRoot = RemoveRedundantCheckedExpression(root, checkedExpression);
                            return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
                        }),
                    context.Diagnostics);
            }
        }

        private static SyntaxNode RemoveRedundantUnsafe(SyntaxNode root, SyntaxToken token)
        {
            if (!(token.Parent is UnsafeStatementSyntax unsafeStatement))
            {
                return RemoveRedundantToken(root, token);
            }

            if (unsafeStatement.Parent is BlockSyntax parentBlock &&
                parentBlock.Statements.Count == 1)
            {
                return root.ReplaceNode(
                    parentBlock,
                    parentBlock.WithStatements(unsafeStatement.Block.Statements).WithAdditionalAnnotations(Formatter.Annotation));
            }

            return root.ReplaceNode(
                unsafeStatement,
                unsafeStatement.Block.WithAdditionalAnnotations(Formatter.Annotation));
        }

        private static SyntaxNode RemoveRedundantToken(SyntaxNode root, SyntaxToken token)
        {
            var oldParent = token.Parent;
            var newParent = oldParent.ReplaceToken(
                token,
                SyntaxFactory.Token(SyntaxKind.None));

            return root.ReplaceNode(
                oldParent,
                newParent.WithLeadingTrivia(oldParent.GetLeadingTrivia()));
        }

        private static SyntaxNode RemoveRedundantCheckedStatement(SyntaxNode root, CheckedStatementSyntax checkedStatement)
        {
            var newBlock = SyntaxFactory.Block(checkedStatement.Block.Statements);

            return root.ReplaceNode(
                checkedStatement,
                newBlock.WithTriviaFrom(checkedStatement));
        }

        private static SyntaxNode RemoveRedundantCheckedExpression(SyntaxNode root, CheckedExpressionSyntax checkedExpression)
        {
            return root.ReplaceNode(
                checkedExpression,
                checkedExpression.Expression.WithTriviaFrom(checkedExpression));
        }
    }
}

