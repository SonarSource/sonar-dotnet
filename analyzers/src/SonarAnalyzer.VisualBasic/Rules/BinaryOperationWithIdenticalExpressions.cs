/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    public sealed class BinaryOperationWithIdenticalExpressions : BinaryOperationWithIdenticalExpressionsBase
    {
        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, "{0}");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        private static readonly SyntaxKind[] SyntaxKindsToCheckBinary =
        {
            SyntaxKind.SubtractExpression,
            SyntaxKind.DivideExpression, SyntaxKind.ModuloExpression, SyntaxKind.IntegerDivideExpression,
            SyntaxKind.OrElseExpression, SyntaxKind.AndAlsoExpression,
            SyntaxKind.OrExpression, SyntaxKind.AndExpression, SyntaxKind.ExclusiveOrExpression,
            SyntaxKind.EqualsExpression, SyntaxKind.NotEqualsExpression,
            SyntaxKind.LessThanExpression, SyntaxKind.LessThanOrEqualExpression,
            SyntaxKind.GreaterThanExpression, SyntaxKind.GreaterThanOrEqualExpression
        };

        private static readonly SyntaxKind[] SyntaxKindsToCheckAssignment =
        {
            SyntaxKind.SubtractAssignmentStatement,
            SyntaxKind.DivideAssignmentStatement,
            SyntaxKind.IntegerDivideAssignmentStatement
        };

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(
                c =>
                {
                    var expression = (BinaryExpressionSyntax)c.Node;
                    ReportIfExpressionsMatch(c, expression.Left, expression.Right, expression.OperatorToken);
                },
                SyntaxKindsToCheckBinary);

            context.RegisterNodeAction(
                c =>
                {
                    var expression = (AssignmentStatementSyntax)c.Node;
                    ReportIfExpressionsMatch(c, expression.Left, expression.Right, expression.OperatorToken);
                },
                SyntaxKindsToCheckAssignment);
        }

        private static void ReportIfExpressionsMatch(SonarSyntaxNodeReportingContext context, ExpressionSyntax left, ExpressionSyntax right,
            SyntaxToken operatorToken)
        {
            if (VisualBasicEquivalenceChecker.AreEquivalent(left.RemoveParentheses(), right.RemoveParentheses()))
            {
                var message = string.Format(OperatorMessageFormat, operatorToken);
                context.ReportIssue(CreateDiagnostic(Rule, context.Node.GetLocation(), message));
            }
        }
    }
}
