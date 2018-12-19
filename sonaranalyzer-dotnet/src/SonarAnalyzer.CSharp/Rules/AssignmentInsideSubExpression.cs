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
using System.Collections.Generic;
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
    public sealed class AssignmentInsideSubExpression : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S1121";
        private const string MessageFormat = "Extract the assignment of '{0}' from this expression.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var assignment = (AssignmentExpressionSyntax)c.Node;

                    var topParenthesizedExpression = assignment.GetSelfOrTopParenthesizedExpression();

                    if (IsNonCompliantSubExpression(assignment, topParenthesizedExpression) ||
                        IsDirectlyInStatementCondition(assignment, topParenthesizedExpression))
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, assignment.OperatorToken.GetLocation(),
                            assignment.Left.ToString()));
                    }
                },
                SyntaxKind.SimpleAssignmentExpression,
                SyntaxKind.AddAssignmentExpression,
                SyntaxKind.SubtractAssignmentExpression,
                SyntaxKind.MultiplyAssignmentExpression,
                SyntaxKind.DivideAssignmentExpression,
                SyntaxKind.ModuloAssignmentExpression,
                SyntaxKind.AndAssignmentExpression,
                SyntaxKind.ExclusiveOrAssignmentExpression,
                SyntaxKind.OrAssignmentExpression,
                SyntaxKind.LeftShiftAssignmentExpression,
                SyntaxKind.RightShiftAssignmentExpression);
        }

        private static bool IsNonCompliantSubExpression(AssignmentExpressionSyntax assignment, ExpressionSyntax topParenthesizedExpression)
        {
            return IsInsideExpression(topParenthesizedExpression) &&
                !IsCompliantAssignmentInsideExpression(assignment, topParenthesizedExpression);
        }

        private static bool IsInsideExpression(ExpressionSyntax expression)
        {
            return expression.Parent.FirstAncestorOrSelf<ExpressionSyntax>() != null;
        }

        private static bool IsCompliantAssignmentInsideExpression(AssignmentExpressionSyntax assignment, ExpressionSyntax topParenthesizedExpression)
        {
            var expressionParent = topParenthesizedExpression.Parent.FirstAncestorOrSelf<ExpressionSyntax>();
            if (expressionParent == null)
            {
                // not inside an expression
                return true;
            }

            if (IsCompliantCoalesceExpression(expressionParent, assignment))
            {
                return true;
            }

            if (RelationalExpressionKinds.Contains(expressionParent.Kind()) &&
                IsInStatementCondition(expressionParent))
            {
                return true;
            }

            return AllowedParentExpressionKinds.Contains(expressionParent.Kind());
        }

        private static bool IsCompliantCoalesceExpression(ExpressionSyntax parentExpression, AssignmentExpressionSyntax assignment)
        {

            return assignment.IsKind(SyntaxKind.SimpleAssignmentExpression) &&
                TryGetCoalesceExpressionParent(parentExpression, out var coalesceExpression) &&
                CSharpEquivalenceChecker.AreEquivalent(assignment.Left.RemoveParentheses(), coalesceExpression.Left.RemoveParentheses());
        }

        private static bool TryGetCoalesceExpressionParent(ExpressionSyntax parent, out BinaryExpressionSyntax coalesceExpression)
        {
            coalesceExpression = null;

            var currentParent = parent;
            while (currentParent != null &&
                !TryGetCoalesceExpression(currentParent, out coalesceExpression))
            {
                currentParent = currentParent.Parent as ExpressionSyntax;
            }

            return currentParent != null;
        }

        private static bool TryGetCoalesceExpression(ExpressionSyntax expression, out BinaryExpressionSyntax coalesceExpression)
        {
            coalesceExpression = expression as BinaryExpressionSyntax;
            return coalesceExpression != null && coalesceExpression.IsKind(SyntaxKind.CoalesceExpression);
        }

        private static bool IsDirectlyInStatementCondition(ExpressionSyntax expression, ExpressionSyntax topParenthesizedExpression)
        {
            return IsDirectlyInStatementCondition<IfStatementSyntax>(topParenthesizedExpression, expression, s => s.Condition) ||
                IsDirectlyInStatementCondition<ForStatementSyntax>(topParenthesizedExpression, expression, s => s.Condition) ||
                IsDirectlyInStatementCondition<WhileStatementSyntax>(topParenthesizedExpression, expression, s => s.Condition) ||
                IsDirectlyInStatementCondition<DoStatementSyntax>(topParenthesizedExpression, expression, s => s.Condition);
        }

        private static bool IsInStatementCondition(ExpressionSyntax expression)
        {
            var expressionOrParenthesizedParent = expression.GetSelfOrTopParenthesizedExpression();

            return IsInStatementCondition<IfStatementSyntax>(expressionOrParenthesizedParent, expression, s => s?.Condition) ||
                IsInStatementCondition<ForStatementSyntax>(expressionOrParenthesizedParent, expression, s => s?.Condition) ||
                IsInStatementCondition<WhileStatementSyntax>(expressionOrParenthesizedParent, expression, s => s?.Condition) ||
                IsInStatementCondition<DoStatementSyntax>(expressionOrParenthesizedParent, expression, s => s?.Condition);
        }

        private static bool IsDirectlyInStatementCondition<T>(ExpressionSyntax expressionParent,
            ExpressionSyntax originalExpression, Func<T, ExpressionSyntax> conditionSelector) where T : SyntaxNode
        {
            var statement = expressionParent.Parent.FirstAncestorOrSelf<T>();
            return statement != null &&
                conditionSelector(statement).RemoveParentheses() == originalExpression;
        }

        private static bool IsInStatementCondition<T>(ExpressionSyntax expressionParent, ExpressionSyntax originalExpression,
            Func<T, ExpressionSyntax> conditionSelector) where T : SyntaxNode
        {
            var statement = expressionParent.Parent.FirstAncestorOrSelf<T>();
            var condition = conditionSelector(statement);
            return condition != null
                && condition.Contains(originalExpression);
        }

        private static readonly ISet<SyntaxKind> AllowedParentExpressionKinds = new HashSet<SyntaxKind>
        {
            SyntaxKind.SimpleAssignmentExpression,
            SyntaxKind.ParenthesizedLambdaExpression,
            SyntaxKind.SimpleLambdaExpression,
            SyntaxKind.AnonymousMethodExpression,
            SyntaxKind.ObjectInitializerExpression
        };

        private static readonly ISet<SyntaxKind> RelationalExpressionKinds = new HashSet<SyntaxKind>
        {
            SyntaxKind.EqualsExpression, SyntaxKind.NotEqualsExpression,
            SyntaxKind.LessThanExpression, SyntaxKind.LessThanOrEqualExpression,
            SyntaxKind.GreaterThanExpression, SyntaxKind.GreaterThanOrEqualExpression
        };
    }
}
