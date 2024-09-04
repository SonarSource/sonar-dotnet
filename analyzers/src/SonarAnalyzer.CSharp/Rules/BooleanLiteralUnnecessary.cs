/*
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

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class BooleanLiteralUnnecessary : BooleanLiteralUnnecessaryBase<SyntaxKind>
    {
        protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

        protected override SyntaxToken? GetOperatorToken(SyntaxNode node) =>
            node switch
            {
                BinaryExpressionSyntax binary => binary.OperatorToken,
                _ when IsPatternExpressionSyntaxWrapper.IsInstance(node) => ((IsPatternExpressionSyntaxWrapper)node).IsKeyword,
                _ => null,
            };

        protected override bool IsTrue(SyntaxNode syntaxNode) => syntaxNode.IsTrue();

        protected override bool IsFalse(SyntaxNode syntaxNode) => syntaxNode.IsFalse();

        protected override bool IsInsideTernaryWithThrowExpression(SyntaxNode syntaxNode) =>
            syntaxNode.Parent is ConditionalExpressionSyntax conditionalExpression
            && (IsThrowExpression(conditionalExpression.WhenTrue) || IsThrowExpression(conditionalExpression.WhenFalse));

        protected override SyntaxNode GetLeftNode(SyntaxNode node) =>
            node switch
            {
                BinaryExpressionSyntax binaryExpression => binaryExpression.Left,
                _ when IsPatternExpressionSyntaxWrapper.IsInstance(node) => ((IsPatternExpressionSyntaxWrapper)node).Expression,
                _ => null
            };

        protected override SyntaxNode GetRightNode(SyntaxNode node) =>
            node switch
            {
                BinaryExpressionSyntax binaryExpression => binaryExpression.Right,
                _ when IsPatternExpressionSyntaxWrapper.IsInstance(node) => ((IsPatternExpressionSyntaxWrapper)node).Pattern,
                _ => null
            };

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(CheckLogicalNot, SyntaxKind.LogicalNotExpression);
            context.RegisterNodeAction(CheckAndExpression, SyntaxKind.LogicalAndExpression);
            context.RegisterNodeAction(CheckOrExpression, SyntaxKind.LogicalOrExpression);
            context.RegisterNodeAction(CheckEquals, SyntaxKind.EqualsExpression, SyntaxKindEx.IsPatternExpression);
            context.RegisterNodeAction(CheckNotEquals, SyntaxKind.NotEqualsExpression);
            context.RegisterNodeAction(CheckConditional, SyntaxKind.ConditionalExpression);
            context.RegisterNodeAction(CheckForLoopCondition, SyntaxKind.ForStatement);
            base.Initialize(context);
        }

        private void CheckForLoopCondition(SonarSyntaxNodeReportingContext context)
        {
            var forLoop = (ForStatementSyntax)context.Node;

            if (forLoop.Condition != null
                && CSharpEquivalenceChecker.AreEquivalent(forLoop.Condition.RemoveParentheses(), SyntaxConstants.TrueLiteralExpression))
            {
                context.ReportIssue(Rule, forLoop.Condition);
            }
        }

        private void CheckLogicalNot(SonarSyntaxNodeReportingContext context)
        {
            var logicalNot = (PrefixUnaryExpressionSyntax)context.Node;
            var logicalNotOperand = logicalNot.Operand.RemoveParentheses();
            if (IsTrue(logicalNotOperand) || IsFalse(logicalNotOperand))
            {
                context.ReportIssue(Rule, logicalNot.Operand);
            }
        }

        private void CheckConditional(SonarSyntaxNodeReportingContext context)
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
            CheckTernaryExpressionBranches(context, whenTrue, whenFalse);
        }

        private static bool IsThrowExpression(ExpressionSyntax expressionSyntax) =>
            ThrowExpressionSyntaxWrapper.IsInstance(expressionSyntax);
    }
}
