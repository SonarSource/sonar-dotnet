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
    public sealed class BooleanLiteralUnnecessaryCodeFixProvider : SonarCodeFixProvider
    {
        internal const string Title = "Remove the unnecessary Boolean literal(s)";
        public override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create(BooleanLiteralUnnecessary.DiagnosticId);
            }
        }
        public override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        protected override Task RegisterCodeFixesAsync(SyntaxNode root, CodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            if (!(root.FindNode(diagnosticSpan, getInnermostNodeForTie: true) is ExpressionSyntax syntaxNode))
            {
                return TaskHelper.CompletedTask;
            }

            var parent = syntaxNode.Parent;
            syntaxNode = syntaxNode.RemoveParentheses();

            if (syntaxNode is BinaryExpressionSyntax binary)
            {
                RegisterBinaryExpressionReplacement(context, root, syntaxNode, binary);
                return TaskHelper.CompletedTask;
            }

            if (syntaxNode is ConditionalExpressionSyntax conditional)
            {
                RegisterConditionalExpressionRemoval(context, root, conditional);
                return TaskHelper.CompletedTask;
            }

            if (!(syntaxNode is LiteralExpressionSyntax literal))
            {
                return TaskHelper.CompletedTask;
            }

            if (parent is PrefixUnaryExpressionSyntax)
            {
                RegisterBooleanInversion(context, root, literal);
                return TaskHelper.CompletedTask;
            }

            if (parent is ConditionalExpressionSyntax conditionalParent)
            {
                RegisterConditionalExpressionRewrite(context, root, literal, conditionalParent);
                return TaskHelper.CompletedTask;
            }

            if (parent is BinaryExpressionSyntax binaryParent)
            {
                RegisterBinaryExpressionRemoval(context, root, literal, binaryParent);
                return TaskHelper.CompletedTask;
            }

            if (parent is ForStatementSyntax forStatement)
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
                (CSharpEquivalenceChecker.AreEquivalent(binary.Left, CSharpSyntaxHelper.FalseLiteralExpression) ||
                 CSharpEquivalenceChecker.AreEquivalent(binary.Right, CSharpSyntaxHelper.FalseLiteralExpression)))
            {
                return CSharpSyntaxHelper.FalseLiteralExpression;
            }
            if (binary.IsKind(SyntaxKind.LogicalOrExpression) &&
                (CSharpEquivalenceChecker.AreEquivalent(binary.Left, CSharpSyntaxHelper.TrueLiteralExpression) ||
                 CSharpEquivalenceChecker.AreEquivalent(binary.Right, CSharpSyntaxHelper.TrueLiteralExpression)))
            {
                return CSharpSyntaxHelper.TrueLiteralExpression;
            }

            #endregion

            #region ==/!= both sides booleans

            if (binary.IsKind(SyntaxKind.EqualsExpression) &&
                TwoSidesAreDifferentBooleans(binary))
            {
                return CSharpSyntaxHelper.FalseLiteralExpression;
            }
            if (binary.IsKind(SyntaxKind.EqualsExpression) &&
                TwoSidesAreSameBooleans(binary))
            {
                return CSharpSyntaxHelper.TrueLiteralExpression;
            }
            if (binary.IsKind(SyntaxKind.NotEqualsExpression) &&
                TwoSidesAreSameBooleans(binary))
            {
                return CSharpSyntaxHelper.FalseLiteralExpression;
            }
            if (binary.IsKind(SyntaxKind.NotEqualsExpression) &&
                TwoSidesAreDifferentBooleans(binary))
            {
                return CSharpSyntaxHelper.TrueLiteralExpression;
            }

            #endregion

            if (CSharpEquivalenceChecker.AreEquivalent(binary.Left, CSharpSyntaxHelper.TrueLiteralExpression) ||
                CSharpEquivalenceChecker.AreEquivalent(binary.Left, CSharpSyntaxHelper.FalseLiteralExpression))
            {
                return binary.Right;
            }
            return binary.Left;
        }

        private static bool TwoSidesAreDifferentBooleans(BinaryExpressionSyntax binary)
        {
            return (
                CSharpEquivalenceChecker.AreEquivalent(binary.Left, CSharpSyntaxHelper.TrueLiteralExpression) &&
                CSharpEquivalenceChecker.AreEquivalent(binary.Right, CSharpSyntaxHelper.FalseLiteralExpression)) ||
                (
                CSharpEquivalenceChecker.AreEquivalent(binary.Left, CSharpSyntaxHelper.FalseLiteralExpression) &&
                CSharpEquivalenceChecker.AreEquivalent(binary.Right, CSharpSyntaxHelper.TrueLiteralExpression));
        }
        private static bool TwoSidesAreSameBooleans(BinaryExpressionSyntax binary)
        {
            return (
                CSharpEquivalenceChecker.AreEquivalent(binary.Left, CSharpSyntaxHelper.TrueLiteralExpression) &&
                CSharpEquivalenceChecker.AreEquivalent(binary.Right, CSharpSyntaxHelper.TrueLiteralExpression)) ||
                (
                CSharpEquivalenceChecker.AreEquivalent(binary.Left, CSharpSyntaxHelper.FalseLiteralExpression) &&
                CSharpEquivalenceChecker.AreEquivalent(binary.Right, CSharpSyntaxHelper.FalseLiteralExpression));
        }

        private static Document RemovePrefixUnary(Document document, SyntaxNode root,
            SyntaxNode literal)
        {
            if (CSharpEquivalenceChecker.AreEquivalent(literal, CSharpSyntaxHelper.TrueLiteralExpression))
            {
                var newRoot = root.ReplaceNode(literal.Parent, CSharpSyntaxHelper.FalseLiteralExpression);
                return document.WithSyntaxRoot(newRoot);
            }
            else
            {
                var newRoot = root.ReplaceNode(literal.Parent, CSharpSyntaxHelper.TrueLiteralExpression);
                return document.WithSyntaxRoot(newRoot);
            }
        }

        private static Document RemoveConditional(Document document, SyntaxNode root,
            ConditionalExpressionSyntax conditional)
        {
            if (CSharpEquivalenceChecker.AreEquivalent(conditional.WhenTrue, CSharpSyntaxHelper.TrueLiteralExpression))
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
                CSharpEquivalenceChecker.AreEquivalent(syntaxNode, CSharpSyntaxHelper.TrueLiteralExpression))
            {
                var newRoot = ReplaceExpressionWithBinary(conditional, root,
                    SyntaxKind.LogicalOrExpression,
                    conditional.Condition,
                    conditional.WhenFalse);

                return document.WithSyntaxRoot(newRoot);
            }

            if (whenTrue.Equals(syntaxNode) &&
                CSharpEquivalenceChecker.AreEquivalent(syntaxNode, CSharpSyntaxHelper.FalseLiteralExpression))
            {
                var newRoot = ReplaceExpressionWithBinary(conditional, root,
                    SyntaxKind.LogicalAndExpression,
                    GetNegatedExpression(conditional.Condition),
                    conditional.WhenFalse);

                return document.WithSyntaxRoot(newRoot);
            }

            var whenFalse = conditional.WhenFalse.RemoveParentheses();

            if (whenFalse.Equals(syntaxNode) &&
                CSharpEquivalenceChecker.AreEquivalent(syntaxNode, CSharpSyntaxHelper.TrueLiteralExpression))
            {
                var newRoot = ReplaceExpressionWithBinary(conditional, root,
                    SyntaxKind.LogicalOrExpression,
                    GetNegatedExpression(conditional.Condition),
                    conditional.WhenTrue);

                return document.WithSyntaxRoot(newRoot);
            }

            if (whenFalse.Equals(syntaxNode) &&
                CSharpEquivalenceChecker.AreEquivalent(syntaxNode, CSharpSyntaxHelper.FalseLiteralExpression))
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
