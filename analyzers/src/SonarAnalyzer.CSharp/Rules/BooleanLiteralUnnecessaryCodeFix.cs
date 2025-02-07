/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;

namespace SonarAnalyzer.Rules.CSharp
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    public sealed class BooleanLiteralUnnecessaryCodeFix : SonarCodeFix
    {
        internal const string Title = "Remove the unnecessary Boolean literal(s)";
        public override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(BooleanLiteralUnnecessary.DiagnosticId);

        protected override Task RegisterCodeFixesAsync(SyntaxNode root, SonarCodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            if (root.FindNode(diagnosticSpan, getInnermostNodeForTie: true) is not ExpressionSyntax syntaxNode)
            {
                return Task.CompletedTask;
            }

            var parent = syntaxNode.Parent;
            syntaxNode = syntaxNode.RemoveParentheses();

            if (syntaxNode is BinaryExpressionSyntax binary)
            {
                RegisterBinaryExpressionReplacement(context, root, syntaxNode, binary);
                return Task.CompletedTask;
            }

            if (syntaxNode is ConditionalExpressionSyntax conditional)
            {
                RegisterConditionalExpressionRemoval(context, root, conditional);
                return Task.CompletedTask;
            }

            if (IsPatternExpressionSyntaxWrapper.IsInstance(syntaxNode))
            {
                RegisterPatternExpressionReplacement(context, root, (IsPatternExpressionSyntaxWrapper)syntaxNode);
            }

            if (syntaxNode is not LiteralExpressionSyntax literal)
            {
                return Task.CompletedTask;
            }

            if (parent is PrefixUnaryExpressionSyntax)
            {
                RegisterBooleanInversion(context, root, literal);
                return Task.CompletedTask;
            }

            if (parent is ConditionalExpressionSyntax conditionalParent)
            {
                RegisterConditionalExpressionRewrite(context, root, literal, conditionalParent);
                return Task.CompletedTask;
            }

            if (parent is BinaryExpressionSyntax binaryParent)
            {
                RegisterBinaryExpressionRemoval(context, root, literal, binaryParent);
                return Task.CompletedTask;
            }

            if (parent is ForStatementSyntax forStatement)
            {
                RegisterForStatementConditionRemoval(context, root, forStatement);
                return Task.CompletedTask;
            }

            return Task.CompletedTask;
        }

        private static void RegisterPatternExpressionReplacement(SonarCodeFixContext context, SyntaxNode root, IsPatternExpressionSyntaxWrapper patternExpression)
        {
            var replacement = patternExpression.Pattern.SyntaxNode.IsTrue()
                ? patternExpression.Expression
                : GetNegatedExpression(patternExpression.Expression);

            if (replacement.IsTrue())
            {
                replacement = SyntaxConstants.TrueLiteralExpression;
            }
            else if (replacement.IsFalse())
            {
                replacement = SyntaxConstants.FalseLiteralExpression;
            }

            context.RegisterCodeFix(
                Title,
                c =>
                {
                    var newRoot = root.ReplaceNode(patternExpression.SyntaxNode, replacement.WithAdditionalAnnotations(Formatter.Annotation));
                    return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
                },
                context.Diagnostics);
        }

        private static void RegisterForStatementConditionRemoval(SonarCodeFixContext context, SyntaxNode root, ForStatementSyntax forStatement) =>
            context.RegisterCodeFix(
                Title,
                c =>
                {
                    var newRoot = root.ReplaceNode(
                        forStatement,
                        forStatement.WithCondition(null).WithAdditionalAnnotations(Formatter.Annotation));

                    return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
                },
                context.Diagnostics);

        private static void RegisterBinaryExpressionRemoval(SonarCodeFixContext context, SyntaxNode root, LiteralExpressionSyntax literal, BinaryExpressionSyntax binaryParent)
        {
            var otherNode = binaryParent.Left.RemoveParentheses().Equals(literal)
                ? binaryParent.Right
                : binaryParent.Left;

            context.RegisterCodeFix(
                Title,
                c =>
                {
                    var newExpression = GetNegatedExpression(otherNode).WithAdditionalAnnotations(Simplifier.Annotation);
                    var newRoot = root.ReplaceNode(binaryParent, newExpression
                        .WithAdditionalAnnotations(Formatter.Annotation));

                    return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
                },
                context.Diagnostics);
        }

        private static void RegisterConditionalExpressionRewrite(SonarCodeFixContext context, SyntaxNode root, LiteralExpressionSyntax literal, ConditionalExpressionSyntax conditionalParent) =>
            context.RegisterCodeFix(
                Title,
                c => Task.FromResult(RewriteConditional(context.Document, root, literal, conditionalParent)),
                context.Diagnostics);

        private static void RegisterBooleanInversion(SonarCodeFixContext context, SyntaxNode root, LiteralExpressionSyntax literal) =>
            context.RegisterCodeFix(
                Title,
                c => Task.FromResult(RemovePrefixUnary(context.Document, root, literal)),
                context.Diagnostics);

        private static void RegisterConditionalExpressionRemoval(SonarCodeFixContext context, SyntaxNode root, ConditionalExpressionSyntax conditional) =>
            context.RegisterCodeFix(
                Title,
                c => Task.FromResult(RemoveConditional(context.Document, root, conditional)),
                context.Diagnostics);

        private static void RegisterBinaryExpressionReplacement(SonarCodeFixContext context, SyntaxNode root, SyntaxNode syntaxNode, BinaryExpressionSyntax binary) =>
            context.RegisterCodeFix(
                Title,
                c =>
                {
                    var keepThisNode = FindNodeToKeep(binary).WithAdditionalAnnotations(Simplifier.Annotation);
                    var newRoot = root.ReplaceNode(syntaxNode, keepThisNode
                        .WithAdditionalAnnotations(Formatter.Annotation));
                    return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
                },
                context.Diagnostics);

        private static SyntaxNode FindNodeToKeep(BinaryExpressionSyntax binary)
        {
            // logical and false, logical or true
            if (binary.IsKind(SyntaxKind.LogicalAndExpression)
                && (CSharpEquivalenceChecker.AreEquivalent(binary.Left, SyntaxConstants.FalseLiteralExpression)
                   || CSharpEquivalenceChecker.AreEquivalent(binary.Right, SyntaxConstants.FalseLiteralExpression)))
            {
                return SyntaxConstants.FalseLiteralExpression;
            }
            if (binary.IsKind(SyntaxKind.LogicalOrExpression)
                && (CSharpEquivalenceChecker.AreEquivalent(binary.Left, SyntaxConstants.TrueLiteralExpression)
                   || CSharpEquivalenceChecker.AreEquivalent(binary.Right, SyntaxConstants.TrueLiteralExpression)))
            {
                return SyntaxConstants.TrueLiteralExpression;
            }

            // ==/!= both sides booleans
            if (binary.IsKind(SyntaxKind.EqualsExpression)
                && TwoSidesAreDifferentBooleans(binary))
            {
                return SyntaxConstants.FalseLiteralExpression;
            }
            if (binary.IsKind(SyntaxKind.EqualsExpression)
                && TwoSidesAreSameBooleans(binary))
            {
                return SyntaxConstants.TrueLiteralExpression;
            }
            if (binary.IsKind(SyntaxKind.NotEqualsExpression)
                && TwoSidesAreSameBooleans(binary))
            {
                return SyntaxConstants.FalseLiteralExpression;
            }
            if (binary.IsKind(SyntaxKind.NotEqualsExpression)
                && TwoSidesAreDifferentBooleans(binary))
            {
                return SyntaxConstants.TrueLiteralExpression;
            }

            // ==/!= one side boolean
            if (binary.IsKind(SyntaxKind.EqualsExpression))
            {
                // edge case [condition == false] -> !condition
                if (CSharpEquivalenceChecker.AreEquivalent(binary.Right, SyntaxConstants.FalseLiteralExpression))
                {
                    return SyntaxFactory.PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, binary.Left);
                }
                // edge case [false == condition] -> !condition
                if (CSharpEquivalenceChecker.AreEquivalent(binary.Left, SyntaxConstants.FalseLiteralExpression))
                {
                    return SyntaxFactory.PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, binary.Right);
                }
            }

            return CSharpEquivalenceChecker.AreEquivalent(binary.Left, SyntaxConstants.TrueLiteralExpression)
                   || CSharpEquivalenceChecker.AreEquivalent(binary.Left, SyntaxConstants.FalseLiteralExpression)
                ? binary.Right
                : binary.Left;
        }

        private static bool TwoSidesAreDifferentBooleans(BinaryExpressionSyntax binary) =>
            (CSharpEquivalenceChecker.AreEquivalent(binary.Left, SyntaxConstants.TrueLiteralExpression)
             && CSharpEquivalenceChecker.AreEquivalent(binary.Right, SyntaxConstants.FalseLiteralExpression))
            || (CSharpEquivalenceChecker.AreEquivalent(binary.Left, SyntaxConstants.FalseLiteralExpression)
               && CSharpEquivalenceChecker.AreEquivalent(binary.Right, SyntaxConstants.TrueLiteralExpression));

        private static bool TwoSidesAreSameBooleans(BinaryExpressionSyntax binary) =>
            (CSharpEquivalenceChecker.AreEquivalent(binary.Left, SyntaxConstants.TrueLiteralExpression)
             && CSharpEquivalenceChecker.AreEquivalent(binary.Right, SyntaxConstants.TrueLiteralExpression))
            || (CSharpEquivalenceChecker.AreEquivalent(binary.Left, SyntaxConstants.FalseLiteralExpression)
               && CSharpEquivalenceChecker.AreEquivalent(binary.Right, SyntaxConstants.FalseLiteralExpression));

        private static Document RemovePrefixUnary(Document document, SyntaxNode root, SyntaxNode literal)
        {
            if (CSharpEquivalenceChecker.AreEquivalent(literal, SyntaxConstants.TrueLiteralExpression))
            {
                var newRoot = root.ReplaceNode(literal.Parent, SyntaxConstants.FalseLiteralExpression);
                return document.WithSyntaxRoot(newRoot);
            }
            else
            {
                var newRoot = root.ReplaceNode(literal.Parent, SyntaxConstants.TrueLiteralExpression);
                return document.WithSyntaxRoot(newRoot);
            }
        }

        private static Document RemoveConditional(Document document, SyntaxNode root, ConditionalExpressionSyntax conditional)
        {
            if (CSharpEquivalenceChecker.AreEquivalent(conditional.WhenTrue, SyntaxConstants.TrueLiteralExpression))
            {
                var newRoot = root.ReplaceNode(
                    conditional,
                    conditional.Condition.WithAdditionalAnnotations(Formatter.Annotation));
                return document.WithSyntaxRoot(newRoot);
            }
            else
            {
                var newRoot = root.ReplaceNode(
                    conditional,
                    GetNegatedExpression(conditional.Condition).WithAdditionalAnnotations(Formatter.Annotation));
                return document.WithSyntaxRoot(newRoot);
            }
        }

        private static SyntaxNode ReplaceExpressionWithBinary(SyntaxNode nodeToReplace, SyntaxNode root, SyntaxKind binaryKind, ExpressionSyntax left, ExpressionSyntax right) =>
            root.ReplaceNode(
                nodeToReplace,
                SyntaxFactory.BinaryExpression(binaryKind, left, right).WithAdditionalAnnotations(Formatter.Annotation));

        private static Document RewriteConditional(Document document, SyntaxNode root, SyntaxNode syntaxNode, ConditionalExpressionSyntax conditional)
        {
            var whenTrue = conditional.WhenTrue.RemoveParentheses();
            if (whenTrue.Equals(syntaxNode) && syntaxNode.IsTrue())
            {
                var newRoot = ReplaceExpressionWithBinary(
                    conditional,
                    root,
                    SyntaxKind.LogicalOrExpression,
                    conditional.Condition,
                    AddParenthesis(conditional.WhenFalse));

                return document.WithSyntaxRoot(newRoot);
            }

            if (whenTrue.Equals(syntaxNode) && syntaxNode.IsFalse())
            {
                var newRoot = ReplaceExpressionWithBinary(
                    conditional,
                    root,
                    SyntaxKind.LogicalAndExpression,
                    GetNegatedExpression(conditional.Condition),
                    AddParenthesis(conditional.WhenFalse));

                return document.WithSyntaxRoot(newRoot);
            }

            var whenFalse = conditional.WhenFalse.RemoveParentheses();

            if (whenFalse.Equals(syntaxNode) && syntaxNode.IsTrue())
            {
                var newRoot = ReplaceExpressionWithBinary(
                    conditional,
                    root,
                    SyntaxKind.LogicalOrExpression,
                    GetNegatedExpression(conditional.Condition),
                    AddParenthesis(conditional.WhenTrue));

                return document.WithSyntaxRoot(newRoot);
            }

            if (whenFalse.Equals(syntaxNode) && syntaxNode.IsFalse())
            {
                var newRoot = ReplaceExpressionWithBinary(
                    conditional,
                    root,
                    SyntaxKind.LogicalAndExpression,
                    conditional.Condition,
                    AddParenthesis(conditional.WhenTrue));

                return document.WithSyntaxRoot(newRoot);
            }

            return document;
        }

        private static ExpressionSyntax GetNegatedExpression(ExpressionSyntax expression) =>
            SyntaxFactory.PrefixUnaryExpression(SyntaxKind.LogicalNotExpression, AddParenthesis(expression));

        private static ExpressionSyntax AddParenthesis(ExpressionSyntax expression) =>
            SyntaxFactory.ParenthesizedExpression(expression).WithAdditionalAnnotations(Simplifier.Annotation);
    }
}
