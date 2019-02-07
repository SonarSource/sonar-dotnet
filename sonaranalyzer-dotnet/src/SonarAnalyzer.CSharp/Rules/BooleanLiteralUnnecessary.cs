/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Helpers.CSharp;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class BooleanLiteralUnnecessary : BooleanLiteralUnnecessaryBase<BinaryExpressionSyntax, SyntaxKind>
    {
        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);
        protected override SyntaxKind TrueLiteral { get; } = SyntaxKind.TrueLiteralExpression;
        protected override SyntaxKind FalseLiteral { get; } = SyntaxKind.FalseLiteralExpression;

        protected override bool IsBooleanLiteral(SyntaxNode node) => node.IsKind(TrueLiteral) || node.IsKind(FalseLiteral);

        protected override SyntaxNode GetLeftNode(BinaryExpressionSyntax binaryExpression) => binaryExpression.Left.RemoveParentheses();

        protected override SyntaxNode GetRightNode(BinaryExpressionSyntax binaryExpression) => binaryExpression.Right.RemoveParentheses();

        protected override SyntaxToken GetOperatorToken(BinaryExpressionSyntax binaryExpression) => binaryExpression.OperatorToken;

        protected override bool IsKind(SyntaxNode syntaxNode, SyntaxKind syntaxKind) => syntaxNode.IsKind(syntaxKind);

        protected override SyntaxNode RemoveParentheses(SyntaxNode syntaxNode) => syntaxNode.RemoveParentheses();

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                CheckLogicalNot,
                SyntaxKind.LogicalNotExpression);

            context.RegisterSyntaxNodeActionInNonGenerated(
                CheckAndExpression,
                SyntaxKind.LogicalAndExpression);

            context.RegisterSyntaxNodeActionInNonGenerated(
                CheckOrExpression,
                SyntaxKind.LogicalOrExpression);

            context.RegisterSyntaxNodeActionInNonGenerated(
                CheckEquals,
                SyntaxKind.EqualsExpression);

            context.RegisterSyntaxNodeActionInNonGenerated(
                CheckNotEquals,
                SyntaxKind.NotEqualsExpression);

            context.RegisterSyntaxNodeActionInNonGenerated(
                CheckConditional,
                SyntaxKind.ConditionalExpression);

            context.RegisterSyntaxNodeActionInNonGenerated(
                CheckForLoopCondition,
                SyntaxKind.ForStatement);
        }

        private static void CheckForLoopCondition(SyntaxNodeAnalysisContext context)
        {
            var forLoop = (ForStatementSyntax)context.Node;

            if (forLoop.Condition != null &&
                CSharpEquivalenceChecker.AreEquivalent(forLoop.Condition.RemoveParentheses(), CSharpSyntaxHelper.TrueLiteralExpression))
            {
                context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, forLoop.Condition.GetLocation()));
            }
        }

        private void CheckLogicalNot(SyntaxNodeAnalysisContext context)
        {
            var logicalNot = (PrefixUnaryExpressionSyntax)context.Node;
            var logicalNotOperand = logicalNot.Operand.RemoveParentheses();
            if (IsBooleanLiteral(logicalNotOperand))
            {
                context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, logicalNot.Operand.GetLocation()));
            }
            bool IsBooleanLiteral(SyntaxNode node) =>
                node.IsKind(SyntaxKind.TrueLiteralExpression) || node.IsKind(SyntaxKind.FalseLiteralExpression);
        }

        private void CheckConditional(SyntaxNodeAnalysisContext context)
        {
            var conditional = (ConditionalExpressionSyntax)context.Node;
            var whenTrue = conditional.WhenTrue;
            var whenFalse = conditional.WhenFalse;
            var typeLeft = context.SemanticModel.GetTypeInfo(whenTrue).Type;
            var typeRight = context.SemanticModel.GetTypeInfo(whenFalse).Type;
            if (ShouldNotReport(typeLeft, typeRight))
            {
                return;
            }
            CheckTernaryExpressionBranches(context, conditional.SyntaxTree, whenTrue, whenFalse);
        }
    }
}
