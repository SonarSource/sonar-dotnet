/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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
using SonarAnalyzer.Extensions;
using SonarAnalyzer.Helpers;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Rules.CSharp
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    public sealed class RedundantNullCheckCodeFixProvider : SonarCodeFixProvider
    {
        internal const string Title = "Remove this unnecessary null check";

        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(RedundantNullCheck.DiagnosticId);

        public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        protected override Task RegisterCodeFixesAsync(SyntaxNode root, CodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var diagnosticNode = root.FindNode(diagnosticSpan, getInnermostNodeForTie: true);

            if (diagnosticNode is BinaryExpressionSyntax nullCheckNode)
            {
                RegisterBinaryExpressionCodeFix(context, root, nullCheckNode);
            }
            else if (diagnosticNode is PrefixUnaryExpressionSyntax prefixUnary && prefixUnary.IsKind(SyntaxKind.LogicalNotExpression))
            {
                RegisterBinaryExpressionCodeFix(context, root, prefixUnary);
            }
            else if (IsPatternExpressionSyntaxWrapper.IsInstance(diagnosticNode))
            {
                var isPatternExpression = (IsPatternExpressionSyntaxWrapper)diagnosticNode.RemoveParentheses();
                if (isPatternExpression.IsNull() || isPatternExpression.IsNotNull())
                {
                    RegisterBinaryExpressionCodeFix(context, root, isPatternExpression.SyntaxNode);
                }
            }
            else if (PatternSyntaxWrapper.IsInstance(diagnosticNode))
            {
                RegisterBinaryPatternCodeFix(context, root, ((PatternSyntaxWrapper)diagnosticNode).SyntaxNode);
            }

            return TaskHelper.CompletedTask;
        }

        private static void RegisterBinaryExpressionCodeFix(CodeFixContext context, SyntaxNode root, SyntaxNode mustBeReplaced) =>
            context.RegisterCodeFix(
                CodeAction.Create(
                    Title,
                    c =>
                    {
                        var binaryExpression = mustBeReplaced.Parent.FirstAncestorOrSelf<BinaryExpressionSyntax>();
                        var newRoot = root;
                        if (binaryExpression != null)
                        {
                            newRoot = ReplaceNode(root, binaryExpression, binaryExpression.Left, binaryExpression.Right, mustBeReplaced);
                        }
                        return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
                    }),
                context.Diagnostics);

        private static void RegisterBinaryPatternCodeFix(CodeFixContext context, SyntaxNode root, SyntaxNode mustBeReplaced) =>
            context.RegisterCodeFix(
                CodeAction.Create(
                    Title,
                    c =>
                    {
                        var binaryExpression = mustBeReplaced.Parent.FirstAncestorOrSelf<SyntaxNode>(n => BinaryPatternSyntaxWrapper.IsInstance(n));
                        var newRoot = root;
                        if (binaryExpression != null)
                        {
                            var binaryPatternNode = (BinaryPatternSyntaxWrapper)binaryExpression;
                            newRoot = ReplaceNode(root, binaryExpression, binaryPatternNode.Left.SyntaxNode, binaryPatternNode.Right.SyntaxNode, mustBeReplaced);
                        }
                        return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
                    }),
                context.Diagnostics);

        private static SyntaxNode ReplaceNode(SyntaxNode root, SyntaxNode binaryExpression, SyntaxNode binaryLeft, SyntaxNode binaryRight, SyntaxNode mustBeReplaced) =>
            binaryLeft.RemoveParentheses() == mustBeReplaced
                ? root.ReplaceNode(binaryExpression, binaryRight.WithTriviaFrom(binaryExpression))
                : root.ReplaceNode(binaryExpression, binaryLeft.WithTriviaFrom(binaryExpression));
    }
}
