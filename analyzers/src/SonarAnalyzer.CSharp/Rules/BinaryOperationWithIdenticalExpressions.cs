/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

namespace SonarAnalyzer.CSharp.Rules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class BinaryOperationWithIdenticalExpressions : BinaryOperationWithIdenticalExpressionsBase
    {
        internal static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, "{0}");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        private static readonly SyntaxKind[] SyntaxKindsToCheckBinary =
        {
            SyntaxKind.SubtractExpression,
            SyntaxKind.DivideExpression, SyntaxKind.ModuloExpression,
            SyntaxKind.LogicalOrExpression, SyntaxKind.LogicalAndExpression,
            SyntaxKind.BitwiseOrExpression, SyntaxKind.BitwiseAndExpression, SyntaxKind.ExclusiveOrExpression,
            SyntaxKind.EqualsExpression, SyntaxKind.NotEqualsExpression,
            SyntaxKind.LessThanExpression, SyntaxKind.LessThanOrEqualExpression,
            SyntaxKind.GreaterThanExpression, SyntaxKind.GreaterThanOrEqualExpression
        };

        private static readonly SyntaxKind[] SyntaxKindsToCheckAssignment =
        {
            SyntaxKind.SubtractAssignmentExpression,
            SyntaxKind.DivideAssignmentExpression, SyntaxKind.ModuloAssignmentExpression,
            SyntaxKind.OrAssignmentExpression, SyntaxKind.AndAssignmentExpression, SyntaxKind.ExclusiveOrAssignmentExpression
        };

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(
                c =>
                {
                    var expression = (BinaryExpressionSyntax)c.Node;
                    ReportIfOperatorExpressionsMatch(c, expression.Left, expression.Right, expression.OperatorToken);
                },
                SyntaxKindsToCheckBinary);

            context.RegisterNodeAction(
                c =>
                {
                    var expression = (AssignmentExpressionSyntax)c.Node;
                    ReportIfOperatorExpressionsMatch(c, expression.Left, expression.Right, expression.OperatorToken);
                },
                SyntaxKindsToCheckAssignment);

            context.RegisterNodeAction(
                c => ReportOnObjectEqualsMatches(c, (InvocationExpressionSyntax)c.Node),
                SyntaxKind.InvocationExpression);
        }

        private static void ReportOnObjectEqualsMatches(SonarSyntaxNodeReportingContext context, InvocationExpressionSyntax invocation)
        {
            var methodSymbol = context.Model.GetSymbolInfo(invocation).Symbol as IMethodSymbol;

            var operands = GetOperands(invocation, methodSymbol);
            if (operands is not null && CSharpEquivalenceChecker.AreEquivalent(RemoveParentheses(operands.Item1), RemoveParentheses(operands.Item2)))
            {
                var message = string.Format(EqualsMessage, operands.Item2);
                context.ReportIssue(Rule, operands.Item1.GetLocation(), [operands.Item2.ToSecondaryLocation()], message);
            }
        }

        private static Tuple<SyntaxNode, SyntaxNode> GetOperands(InvocationExpressionSyntax invocation,
            IMethodSymbol methodSymbol)
        {
            if (methodSymbol.IsStaticObjectEquals())
            {
                return new Tuple<SyntaxNode, SyntaxNode>(
                    invocation.ArgumentList.Arguments[0],
                    invocation.ArgumentList.Arguments[1]);
            }

            if (methodSymbol.IsObjectEquals())
            {
                var invokingExpression = (invocation.Expression as MemberAccessExpressionSyntax)?.Expression;
                if (invokingExpression != null)
                {
                    return new Tuple<SyntaxNode, SyntaxNode>(
                        invokingExpression,
                        invocation.ArgumentList.Arguments[0].Expression);
                }
            }

            return null;
        }

        private static SyntaxNode RemoveParentheses(SyntaxNode node) =>
            node is ExpressionSyntax expression ? expression.RemoveParentheses() : node;

        private static void ReportIfOperatorExpressionsMatch(SonarSyntaxNodeReportingContext context, ExpressionSyntax left, ExpressionSyntax right, SyntaxToken operatorToken)
        {
            if (CSharpEquivalenceChecker.AreEquivalent(left.RemoveParentheses(), right.RemoveParentheses()))
            {
                var message = string.Format(OperatorMessageFormat, operatorToken);
                context.ReportIssue(Rule, right, [left.ToSecondaryLocation()], message);
            }
        }
    }
}
