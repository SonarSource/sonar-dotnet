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

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Helpers.FlowAnalysis.Common;
using SonarAnalyzer.Helpers.FlowAnalysis.CSharp;
using ExplodedGraph = SonarAnalyzer.Helpers.FlowAnalysis.CSharp.ExplodedGraph;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(S2583DiagnosticId)]
    [Rule(S2589DiagnosticId)]
    public class ConditionEvaluatesToConstant : SonarDiagnosticAnalyzer
    {
        private static readonly ISet<SyntaxKind> OmittedSyntaxKinds = ImmutableHashSet.Create(
            SyntaxKind.LogicalAndExpression,
            SyntaxKind.LogicalOrExpression);

        private static readonly ISet<SyntaxKind> LoopBreakingStatements = ImmutableHashSet.Create(
            SyntaxKind.BreakStatement,
            SyntaxKind.ThrowStatement,
            SyntaxKind.ReturnStatement);

        private static readonly ISet<SyntaxKind> LoopStatements = ImmutableHashSet.Create(
            SyntaxKind.WhileStatement,
            SyntaxKind.DoStatement,
            SyntaxKind.ForStatement);

        private const string S2583DiagnosticId = "S2583"; // Bug
        private const string S2583MessageFormat = "Change this condition so that it does not always evaluate to '{0}'; some subsequent code is never executed.";

        private const string S2589DiagnosticId = "S2589"; // Code smell
        private const string S2589MessageFormat = "Change this condition so that it does not always evaluate to '{0}'.";

        private static readonly DiagnosticDescriptor s2583 =
            DiagnosticDescriptorBuilder.GetDescriptor(S2583DiagnosticId, S2583MessageFormat, RspecStrings.ResourceManager);

        private static readonly DiagnosticDescriptor s2589 =
            DiagnosticDescriptorBuilder.GetDescriptor(S2589DiagnosticId, S2589MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(s2583, s2589);

        protected sealed override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterExplodedGraphBasedAnalysis((e, c) => CheckForRedundantConditions(e, c));
        }

        private static void CheckForRedundantConditions(ExplodedGraph explodedGraph, SyntaxNodeAnalysisContext context)
        {
            var conditionTrue = new HashSet<SyntaxNode>();
            var conditionFalse = new HashSet<SyntaxNode>();

            EventHandler<ConditionEvaluatedEventArgs> collectConditions =
                (sender, args) => CollectConditions(args, conditionTrue, conditionFalse);

            EventHandler explorationEnded =
                (sender, args) => Enumerable.Empty<Diagnostic>()
                    .Union(conditionTrue
                        .Except(conditionFalse)
                        .Where(c => !IsConditionOfLoopWithBreak((ExpressionSyntax)c))
                        .Select(node => GetDiagnostics(node, true)))
                    .Union(conditionFalse
                        .Except(conditionTrue)
                        .Select(node => GetDiagnostics(node, false)))
                    .ToList()
                    .ForEach(d => context.ReportDiagnosticWhenActive(d));

            explodedGraph.ExplorationEnded += explorationEnded;
            explodedGraph.ConditionEvaluated += collectConditions;

            try
            {
                explodedGraph.Walk();
            }
            finally
            {
                explodedGraph.ExplorationEnded -= explorationEnded;
                explodedGraph.ConditionEvaluated -= collectConditions;
            }
        }

        private static bool IsConditionOfLoopWithBreak(ExpressionSyntax constantExpression)
        {
            var loop = constantExpression.RemoveParentheses().Parent;

            var loopBody = (loop as WhileStatementSyntax)?.Statement ??
                (loop as DoStatementSyntax)?.Statement ??
                (loop as ForStatementSyntax)?.Statement;

            if (loopBody == null)
            {
                return false;
            }

            var breakStatements = loopBody.DescendantNodes()
                .Where(IsLoopBreakingStatement);

            return breakStatements
                .Select(GetParentLoop)
                .Any(parentLoop => loop.Equals(parentLoop));
        }

        private static SyntaxNode GetParentLoop(SyntaxNode syntaxNode) =>
            syntaxNode.Ancestors().First(a => a.IsAnyKind(LoopStatements));

        private static bool IsLoopBreakingStatement(SyntaxNode syntaxNode) =>
            syntaxNode.IsAnyKind(LoopBreakingStatements);

        private static Diagnostic GetDiagnostics(SyntaxNode constantNode, bool constantValue)
        {
            var unreachableLocations = GetUnreachableLocations(constantNode, constantValue)
                .ToList();

            var constantText = constantValue.ToString().ToLowerInvariant();

            return unreachableLocations.Count > 0 ?
                Diagnostic.Create(s2583, constantNode.GetLocation(), messageArgs: constantText, additionalLocations: unreachableLocations) :
                Diagnostic.Create(s2589, constantNode.GetLocation(), messageArgs: constantText);
        }

        private static IEnumerable<Location> GetUnreachableLocations(SyntaxNode constantExpression, bool constantValue)
        {
            var unreachableExpressions = GetUnreachableExpressions(constantExpression, constantValue)
                .ToList();

            if (unreachableExpressions.Count > 0)
            {
                yield return Location.Create(
                    constantExpression.SyntaxTree,
                    GetSpan(unreachableExpressions.First(), unreachableExpressions.Last()));
            }
            else
            {
                // When the constant expression is the only child of a if/while/etc. statement
                // it is not reported as unreachable, but regardless we want to check if its
                // parent has an unreachable branch.
                var expression = constantExpression as ExpressionSyntax;
                if (expression != null)
                {
                    unreachableExpressions.Add(expression);
                }
            }

            var statement = GetUnreachableStatement(unreachableExpressions, constantValue);
            if (statement != null)
            {
                yield return statement.GetLocation();
            }
        }

        private static TextSpan GetSpan(SyntaxNode startNode, SyntaxNode endNode)
        {
            if (startNode.Equals(endNode))
            {
                return startNode.Span;
            }
            return TextSpan.FromBounds(startNode.SpanStart, endNode.Span.End);
        }

        private static SyntaxNode GetUnreachableStatement(IEnumerable<ExpressionSyntax> unreachableExpressions, bool constantValue)
        {
            var parent = unreachableExpressions
                .Select(SyntaxHelper.GetSelfOrTopParenthesizedExpression) // unreachable expressions
                .Select(node => node.Parent is BinaryExpressionSyntax
                    ? node.Parent.Parent
                    : node.Parent) // Constant node is the only expression in an if statement condition
                .FirstOrDefault();

            return constantValue
                ? (SyntaxNode)
                    (parent as ConditionalExpressionSyntax)?.WhenFalse ??
                    (parent as IfStatementSyntax)?.Else?.Statement
                : (SyntaxNode)
                    (parent as ConditionalExpressionSyntax)?.WhenTrue ??
                    (parent as IfStatementSyntax)?.Statement ??
                    (parent as WhileStatementSyntax)?.Statement ??
                    (parent as DoStatementSyntax)?.Statement;
        }

        private static IEnumerable<ExpressionSyntax> GetUnreachableExpressions(SyntaxNode constantExpression, bool constantValue)
        {
            // This is ugly with LINQ, hence the loop
            foreach (var current in constantExpression.AncestorsAndSelf())
            {
                if (current.Parent is ParenthesizedExpressionSyntax)
                {
                    continue;
                }

                var binary = current.Parent as BinaryExpressionSyntax;
                if (!IsShortcuttingExpression(binary, constantValue))
                {
                    break;
                }

                if (binary.Left == current)
                {
                    yield return binary.Right;
                }
            }
        }

        private static bool IsShortcuttingExpression(BinaryExpressionSyntax expression, bool constantValueIsTrue)
        {
            return expression != null &&
                (expression.IsKind(SyntaxKind.LogicalAndExpression) && !constantValueIsTrue ||
                    expression.IsKind(SyntaxKind.LogicalOrExpression) && constantValueIsTrue);
        }

        private static void CollectConditions(ConditionEvaluatedEventArgs args, HashSet<SyntaxNode> conditionTrue, HashSet<SyntaxNode> conditionFalse)
        {
            var condition = (args.Condition as ExpressionSyntax).RemoveParentheses() ?? args.Condition;

            if (condition == null ||
                OmittedSyntaxKinds.Contains(condition.Kind()) ||
                IsWhileTrueLoopCondition(condition))
            {
                return;
            }

            if (args.EvaluationValue)
            {
                conditionTrue.Add(condition);
            }
            else
            {
                conditionFalse.Add(condition);
            }
        }

        private static bool IsWhileTrueLoopCondition(SyntaxNode condition)
        {
            return condition.IsKind(SyntaxKind.TrueLiteralExpression) &&
                condition.Parent.IsKind(SyntaxKind.WhileStatement);
        }
    }
}
