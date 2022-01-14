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

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Extensions;
using SonarAnalyzer.Helpers;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class BooleanLiteralUnnecessary : BooleanLiteralUnnecessaryBase<BinaryExpressionSyntax, SyntaxKind>
    {
        protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

        protected override bool IsBooleanLiteral(SyntaxNode node) => IsTrueLiteralKind(node) || IsFalseLiteralKind(node);

        protected override SyntaxNode GetLeftNode(BinaryExpressionSyntax binaryExpression) => binaryExpression.Left;

        protected override SyntaxNode GetRightNode(BinaryExpressionSyntax binaryExpression) => binaryExpression.Right;

        protected override SyntaxToken GetOperatorToken(BinaryExpressionSyntax binaryExpression) => binaryExpression.OperatorToken;

        protected override bool IsTrueLiteralKind(SyntaxNode syntaxNode) => syntaxNode.IsKind(SyntaxKind.TrueLiteralExpression);

        protected override bool IsFalseLiteralKind(SyntaxNode syntaxNode) => syntaxNode.IsKind(SyntaxKind.FalseLiteralExpression);

        protected override bool IsInsideTernaryWithThrowExpression(BinaryExpressionSyntax syntaxNode) =>
            syntaxNode.Parent is ConditionalExpressionSyntax conditionalExpression
            && (IsThrowExpression(conditionalExpression.WhenTrue) || IsThrowExpression(conditionalExpression.WhenFalse));

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(CheckLogicalNot, SyntaxKind.LogicalNotExpression);
            context.RegisterSyntaxNodeActionInNonGenerated(CheckAndExpression, SyntaxKind.LogicalAndExpression);
            context.RegisterSyntaxNodeActionInNonGenerated(CheckOrExpression, SyntaxKind.LogicalOrExpression);
            context.RegisterSyntaxNodeActionInNonGenerated(CheckEquals, SyntaxKind.EqualsExpression);
            context.RegisterSyntaxNodeActionInNonGenerated(CheckNotEquals, SyntaxKind.NotEqualsExpression);
            context.RegisterSyntaxNodeActionInNonGenerated(CheckConditional, SyntaxKind.ConditionalExpression);
            context.RegisterSyntaxNodeActionInNonGenerated(CheckForLoopCondition, SyntaxKind.ForStatement);
        }

        private void CheckForLoopCondition(SyntaxNodeAnalysisContext context)
        {
            var forLoop = (ForStatementSyntax)context.Node;

            if (forLoop.Condition != null
                && CSharpEquivalenceChecker.AreEquivalent(forLoop.Condition.RemoveParentheses(), CSharpSyntaxHelper.TrueLiteralExpression))
            {
                context.ReportIssue(Diagnostic.Create(Rule, forLoop.Condition.GetLocation()));
            }
        }

        private void CheckLogicalNot(SyntaxNodeAnalysisContext context)
        {
            var logicalNot = (PrefixUnaryExpressionSyntax)context.Node;
            var logicalNotOperand = logicalNot.Operand.RemoveParentheses();
            if (IsBooleanLiteral(logicalNotOperand))
            {
                context.ReportIssue(Diagnostic.Create(Rule, logicalNot.Operand.GetLocation()));
            }

            static bool IsBooleanLiteral(SyntaxNode node) =>
                node.IsKind(SyntaxKind.TrueLiteralExpression) || node.IsKind(SyntaxKind.FalseLiteralExpression);
        }

        private void CheckConditional(SyntaxNodeAnalysisContext context)
        {
            var conditional = (ConditionalExpressionSyntax)context.Node;
            var whenTrue = conditional.WhenTrue;
            var whenFalse = conditional.WhenFalse;
            if (IsThrowExpression(whenTrue) || IsThrowExpression(whenFalse))
            {
                return;
            }
            var typeLeft = context.SemanticModel.GetTypeInfo(whenTrue).Type;
            var typeRight = context.SemanticModel.GetTypeInfo(whenFalse).Type;
            if (typeLeft.IsNullableBoolean()
                || typeRight.IsNullableBoolean()
                || typeLeft == null
                || typeRight == null)
            {
                return;
            }
            CheckTernaryExpressionBranches(context, conditional.SyntaxTree, whenTrue, whenFalse);
        }

        private static bool IsThrowExpression(ExpressionSyntax expressionSyntax) =>
            ThrowExpressionSyntaxWrapper.IsInstance(expressionSyntax);
    }
}
