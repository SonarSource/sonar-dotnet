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

namespace SonarAnalyzer.Rules.CSharp
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    public sealed class RedundantNullCheckCodeFix : SonarCodeFix
    {
        internal const string Title = "Remove this unnecessary null check";

        public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create(RedundantNullCheck.DiagnosticId);

        protected override Task RegisterCodeFixesAsync(SyntaxNode root, SonarCodeFixContext context)
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
                RegisterBinaryExpressionCodeFix(context, root, isPatternExpression.SyntaxNode);
            }
            else if (PatternSyntaxWrapper.IsInstance(diagnosticNode))
            {
                RegisterBinaryPatternCodeFix(context, root, ((PatternSyntaxWrapper)diagnosticNode).SyntaxNode);
            }
            else if (diagnosticNode.IsNullLiteral() && diagnosticNode.Parent.IsKind(SyntaxKindEx.ConstantPattern))
            {
                RegisterBinaryPatternCodeFix(context, root, diagnosticNode.Parent);
            }

            return Task.CompletedTask;
        }

        private static void RegisterBinaryExpressionCodeFix(SonarCodeFixContext context, SyntaxNode root, SyntaxNode mustBeReplaced) =>
            context.RegisterCodeFix(
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
                },
                context.Diagnostics);

        private static void RegisterBinaryPatternCodeFix(SonarCodeFixContext context, SyntaxNode root, SyntaxNode mustBeReplaced) =>
            context.RegisterCodeFix(
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
                },
                context.Diagnostics);

        private static SyntaxNode ReplaceNode(SyntaxNode root, SyntaxNode binaryExpression, SyntaxNode binaryLeft, SyntaxNode binaryRight, SyntaxNode mustBeReplaced) =>
            binaryLeft.RemoveParentheses() == mustBeReplaced
                ? root.ReplaceNode(binaryExpression, binaryRight.WithTriviaFrom(binaryExpression))
                : root.ReplaceNode(binaryExpression, binaryLeft.WithTriviaFrom(binaryExpression));
    }
}
