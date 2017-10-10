/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2017 SonarSource SA
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
using SonarAnalyzer.Helpers.CSharp;

namespace SonarAnalyzer.Rules.CSharp
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    public class BooleanLiteralUnnecessaryCodeFixProvider : SonarCodeFixProvider
    {
        internal const string Title = "Remove the unnecessary Boolean literal(s)";
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(BooleanLiteralUnnecessary.DiagnosticId);
            }
        }
        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        protected sealed override Task RegisterCodeFixesAsync(SyntaxNode root, CodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var syntaxNode = root.FindNode(diagnosticSpan, getInnermostNodeForTie: true) as ExpressionSyntax;
            if (syntaxNode == null)
            {
                return TaskHelper.CompletedTask;
            }

            var parent = syntaxNode.Parent;
            syntaxNode = syntaxNode.RemoveParentheses();

            var binary = syntaxNode as BinaryExpressionSyntax;
            if (binary != null)
            {
                RegisterBinaryExpressionReplacement(context, root, syntaxNode, binary);
                return TaskHelper.CompletedTask;
            }

            var conditional = syntaxNode as ConditionalExpressionSyntax;
            if (conditional != null)
            {
                RegisterConditionalExpressionRemoval(context, root, conditional);
                return TaskHelper.CompletedTask;
            }

            var literal = syntaxNode as LiteralExpressionSyntax;
            if (literal == null)
            {
                return TaskHelper.CompletedTask;
            }

            if (parent is PrefixUnaryExpressionSyntax)
            {
                RegisterBooleanInversion(context, root, literal);
                return TaskHelper.CompletedTask;
            }

            var conditionalParent = parent as ConditionalExpressionSyntax;
            if (conditionalParent != null)
            {
                RegisterConditionalExpressionRewrite(context, root, literal, conditionalParent);
                return TaskHelper.CompletedTask;
            }

            var binaryParent = parent as BinaryExpressionSyntax;
            if (binaryParent != null)
            {
                RegisterBinaryExpressionRemoval(context, root, literal, binaryParent);
                return TaskHelper.CompletedTask;
            }

            var forStatement = parent as ForStatementSyntax;
            if (forStatement != null)
            {
                RegisterForStatementConditionRemoval(context, root, forStatement);
                return TaskHelper.CompletedTask;
            }

            return TaskHelper.CompletedTask;
        }

        private static void RegisterForStatementConditionRemoval(CodeFixContext context, SyntaxNode root, ForStatementSyntax forStatement)
        {
            context.RegisterCodeFix(
                CodeAction.Create(
                    Title,
                    c =>
                    {
                        var newRoot = root.ReplaceNode(
                            forStatement,
                            forStatement.WithCondition(null).WithAdditionalAnnotations(Formatter.Annotation));

                        return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
                    }),
                context.Diagnostics);
        }

        private static void RegisterBinaryExpressionRemoval(CodeFixContext context, SyntaxNode root, LiteralExpressionSyntax literal, BinaryExpressionSyntax binaryParent)
        {
            var otherNode = binaryParent.Left.RemoveParentheses().Equals(literal)
                ? binaryParent.Right
                : binaryParent.Left;

            context.RegisterCodeFix(
                CodeAction.Create(
                    Title,
                    c =>
                    {
                        var newExpression = GetNegatedExpression(otherNode);
                        var newRoot = root.ReplaceNode(binaryParent, newExpression
                            .WithAdditionalAnnotations(Formatter.Annotation));

                        return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
                    }),
                context.Diagnostics);
        }

        private static void RegisterConditionalExpressionRewrite(CodeFixContext context, SyntaxNode root, LiteralExpressionSyntax literal, ConditionalExpressionSyntax conditionalParent)
        {
            context.RegisterCodeFix(
                CodeAction.Create(
                    Title,
                    c => Task.FromResult(RewriteConditional(context.Document, root, literal, conditionalParent))),
                context.Diagnostics);
        }

        private static void RegisterBooleanInversion(CodeFixContext context, SyntaxNode root, LiteralExpressionSyntax literal)
        {
            context.RegisterCodeFix(
                CodeAction.Create(
                    Title,
                    c => Task.FromResult(RemovePrefixUnary(context.Document, root, literal))),
                context.Diagnostics);
        }

        private static void RegisterConditionalExpressionRemoval(CodeFixContext context, SyntaxNode root, ConditionalExpressionSyntax conditional)
        {
            context.RegisterCodeFix(
                CodeAction.Create(
                    Title,
                    c => Task.FromResult(RemoveConditional(context.Document, root, conditional))),
                context.Diagnostics);
        }

        private static void RegisterBinaryExpressionReplacement(CodeFixContext context, SyntaxNode root, SyntaxNode syntaxNode, BinaryExpressionSyntax binary)
        {
            context.RegisterCodeFix(
                CodeAction.Create(
                    Title,
                    c =>
                    {
                        var keepThisNode = FindNodeToKeep(binary);
                        var newRoot = root.ReplaceNode(syntaxNode, keepThisNode
                            .WithAdditionalAnnotations(Formatter.Annotation));
                        return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
                    }),
                context.Diagnostics);
        }

        private static SyntaxNode FindNodeToKeep(BinaryExpressionSyntax binary)
        {
            #region logical and false, logical or true

            if (binary.IsKind(SyntaxKind.LogicalAndExpression) &&
                (EquivalenceChecker.AreEquivalent(binary.Left, SyntaxHelper.FalseLiteralExpression) ||
                 EquivalenceChecker.AreEquivalent(binary.Right, SyntaxHelper.FalseLiteralExpression)))
            {
                return SyntaxHelper.FalseLiteralExpression;
            }
            if (binary.IsKind(SyntaxKind.LogicalOrExpression) &&
                (EquivalenceChecker.AreEquivalent(binary.Left, SyntaxHelper.TrueLiteralExpression) ||
                 EquivalenceChecker.AreEquivalent(binary.Right, SyntaxHelper.TrueLiteralExpression)))
            {
                return SyntaxHelper.TrueLiteralExpression;
            }

            #endregion

            #region ==/!= both sides booleans

            if (binary.IsKind(SyntaxKind.EqualsExpression) &&
                TwoSidesAreDifferentBooleans(binary))
            {
                return SyntaxHelper.FalseLiteralExpression;
            }
            if (binary.IsKind(SyntaxKind.EqualsExpression) &&
                TwoSidesAreSameBooleans(binary))
            {
                return SyntaxHelper.TrueLiteralExpression;
            }
            if (binary.IsKind(SyntaxKind.NotEqualsExpression) &&
                TwoSidesAreSameBooleans(binary))
            {
                return SyntaxHelper.FalseLiteralExpression;
            }
            if (binary.IsKind(SyntaxKind.NotEqualsExpression) &&
                TwoSidesAreDifferentBooleans(binary))
            {
                return SyntaxHelper.TrueLiteralExpression;
            }

            #endregion

            if (EquivalenceChecker.AreEquivalent(binary.Left, SyntaxHelper.TrueLiteralExpression) ||
                EquivalenceChecker.AreEquivalent(binary.Left, SyntaxHelper.FalseLiteralExpression))
            {
                return binary.Right;
            }
            return binary.Left;
        }

        private static bool TwoSidesAreDifferentBooleans(BinaryExpressionSyntax binary)
        {
            return (
                EquivalenceChecker.AreEquivalent(binary.Left, SyntaxHelper.TrueLiteralExpression) &&
                EquivalenceChecker.AreEquivalent(binary.Right, SyntaxHelper.FalseLiteralExpression)) ||
                (
                EquivalenceChecker.AreEquivalent(binary.Left, SyntaxHelper.FalseLiteralExpression) &&
                EquivalenceChecker.AreEquivalent(binary.Right, SyntaxHelper.TrueLiteralExpression));
        }
        private static bool TwoSidesAreSameBooleans(BinaryExpressionSyntax binary)
        {
            return (
                EquivalenceChecker.AreEquivalent(binary.Left, SyntaxHelper.TrueLiteralExpression) &&
                EquivalenceChecker.AreEquivalent(binary.Right, SyntaxHelper.TrueLiteralExpression)) ||
                (
                EquivalenceChecker.AreEquivalent(binary.Left, SyntaxHelper.FalseLiteralExpression) &&
                EquivalenceChecker.AreEquivalent(binary.Right, SyntaxHelper.FalseLiteralExpression));
        }

        private static Document RemovePrefixUnary(Document document, SyntaxNode root,
            SyntaxNode literal)
        {
            if (EquivalenceChecker.AreEquivalent(literal, SyntaxHelper.TrueLiteralExpression))
            {
                var newRoot = root.ReplaceNode(literal.Parent, SyntaxHelper.FalseLiteralExpression);
                return document.WithSyntaxRoot(newRoot);
            }
            else
            {
                var newRoot = root.ReplaceNode(literal.Parent, SyntaxHelper.TrueLiteralExpression);
                return document.WithSyntaxRoot(newRoot);
            }
        }

        private static Document RemoveConditional(Document document, SyntaxNode root,
            ConditionalExpressionSyntax conditional)
        {
            if (EquivalenceChecker.AreEquivalent(conditional.WhenTrue, SyntaxHelper.TrueLiteralExpression))
            {
                var newRoot = root.ReplaceNode(conditional,
                    conditional.Condition.WithAdditionalAnnotations(Formatter.Annotation));
                return document.WithSyntaxRoot(newRoot);
            }
            else
            {
                var newRoot = root.ReplaceNode(conditional,
                        GetNegatedExpression(conditional.Condition).WithAdditionalAnnotations(Formatter.Annotation));
                return document.WithSyntaxRoot(newRoot);
            }
        }

        private static SyntaxNode ReplaceExpressionWithBinary(SyntaxNode nodeToReplace, SyntaxNode root,
            SyntaxKind binaryKind, ExpressionSyntax left, ExpressionSyntax right)
        {
            return root.ReplaceNode(nodeToReplace,
                    SyntaxFactory.BinaryExpression(
                        binaryKind,
                        left,
                        right).WithAdditionalAnnotations(Formatter.Annotation));
        }

        private static Document RewriteConditional(Document document, SyntaxNode root, SyntaxNode syntaxNode,
            ConditionalExpressionSyntax conditional)
        {
            var whenTrue = conditional.WhenTrue.RemoveParentheses();
            if (whenTrue.Equals(syntaxNode) &&
                EquivalenceChecker.AreEquivalent(syntaxNode, SyntaxHelper.TrueLiteralExpression))
            {
                var newRoot = ReplaceExpressionWithBinary(conditional, root,
                    SyntaxKind.LogicalOrExpression,
                    conditional.Condition,
                    conditional.WhenFalse);

                return document.WithSyntaxRoot(newRoot);
            }

            if (whenTrue.Equals(syntaxNode) &&
                EquivalenceChecker.AreEquivalent(syntaxNode, SyntaxHelper.FalseLiteralExpression))
            {
                var newRoot = ReplaceExpressionWithBinary(conditional, root,
                    SyntaxKind.LogicalAndExpression,
                    GetNegatedExpression(conditional.Condition),
                    conditional.WhenFalse);

                return document.WithSyntaxRoot(newRoot);
            }

            var whenFalse = conditional.WhenFalse.RemoveParentheses();

            if (whenFalse.Equals(syntaxNode) &&
                EquivalenceChecker.AreEquivalent(syntaxNode, SyntaxHelper.TrueLiteralExpression))
            {
                var newRoot = ReplaceExpressionWithBinary(conditional, root,
                    SyntaxKind.LogicalOrExpression,
                    GetNegatedExpression(conditional.Condition),
                    conditional.WhenTrue);

                return document.WithSyntaxRoot(newRoot);
            }

            if (whenFalse.Equals(syntaxNode) &&
                EquivalenceChecker.AreEquivalent(syntaxNode, SyntaxHelper.FalseLiteralExpression))
            {
                var newRoot = ReplaceExpressionWithBinary(conditional, root,
                    SyntaxKind.LogicalAndExpression,
                    conditional.Condition,
                    conditional.WhenTrue);

                return document.WithSyntaxRoot(newRoot);
            }

            return document;
        }

        private static ExpressionSyntax GetNegatedExpression(ExpressionSyntax expression)
        {
            var exp = expression;
            if (expression is BinaryExpressionSyntax ||
                expression is ConditionalExpressionSyntax)
            {
                exp = SyntaxFactory.ParenthesizedExpression(expression);
            }

            return SyntaxFactory.PrefixUnaryExpression(
                SyntaxKind.LogicalNotExpression,
                exp);
        }
    }
}
