
/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Helpers.CSharp;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class BinaryOperationWithIdenticalExpressions : BinaryOperationWithIdenticalExpressionsBase
    {
        internal static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, "{0}", RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

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
                CSharpEquivalenceChecker.AreEquivalent(RemoveParantheses(operands.Item1), RemoveParantheses(operands.Item2)))
            {
                var message = string.Format(EqualsMessage, operands.Item2);

                context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, operands.Item1.GetLocation(),
                    additionalLocations: new[] { operands.Item2.GetLocation() },
                    messageArgs: message));
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

        private static SyntaxNode RemoveParantheses(SyntaxNode node)
        {
            return !(node is ExpressionSyntax expression) ? node : expression.RemoveParentheses();
        }

        private static void ReportIfOperatorExpressionsMatch(SyntaxNodeAnalysisContext context, ExpressionSyntax left, ExpressionSyntax right,
            SyntaxToken operatorToken)
        {
            if (CSharpEquivalenceChecker.AreEquivalent(left.RemoveParentheses(), right.RemoveParentheses()))
            {
                var message = string.Format(OperatorMessageFormat, operatorToken);

                context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, right.GetLocation(),
                    additionalLocations: new[] { left.GetLocation() },
                    messageArgs: message));
            }
        }
    }
}
