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

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Extensions;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
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
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var expression = (BinaryExpressionSyntax)c.Node;
                    ReportIfOperatorExpressionsMatch(c, expression.Left, expression.Right, expression.OperatorToken);
                },
                SyntaxKindsToCheckBinary);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var expression = (AssignmentExpressionSyntax)c.Node;
                    ReportIfOperatorExpressionsMatch(c, expression.Left, expression.Right, expression.OperatorToken);
                },
                SyntaxKindsToCheckAssignment);

            context.RegisterSyntaxNodeActionInNonGenerated(
                c => ReportOnObjectEqualsMatches((InvocationExpressionSyntax)c.Node, c),
                SyntaxKind.InvocationExpression);
        }

        private static void ReportOnObjectEqualsMatches(InvocationExpressionSyntax invocation,
            SyntaxNodeAnalysisContext context)
        {
            var methodSymbol = context.SemanticModel.GetSymbolInfo(invocation).Symbol as IMethodSymbol;

            var operands = GetOperands(invocation, methodSymbol);
            if (operands != null &&
                CSharpEquivalenceChecker.AreEquivalent(RemoveParentheses(operands.Item1), RemoveParentheses(operands.Item2)))
            {
                var message = string.Format(EqualsMessage, operands.Item2);
                var diagnostic = DiagnosticFactory.Create(Rule, context.Compilation, operands.Item1.GetLocation(), additionalLocations: new[] { operands.Item2.GetLocation() }, messageArgs: message);
                context.ReportIssue(diagnostic);
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
            !(node is ExpressionSyntax expression) ? node : expression.RemoveParentheses();

        private static void ReportIfOperatorExpressionsMatch(SyntaxNodeAnalysisContext context, ExpressionSyntax left, ExpressionSyntax right, SyntaxToken operatorToken)
        {
            if (CSharpEquivalenceChecker.AreEquivalent(left.RemoveParentheses(), right.RemoveParentheses()))
            {
                var message = string.Format(OperatorMessageFormat, operatorToken);
                context.ReportIssue(DiagnosticFactory.Create(Rule, context.Compilation, right.GetLocation(), additionalLocations: new[] { left.GetLocation() }, messageArgs: message));
            }
        }
    }
}
