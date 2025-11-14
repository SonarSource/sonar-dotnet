/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.CSharp.Rules
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
