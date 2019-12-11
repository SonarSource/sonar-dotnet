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

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.ControlFlowGraph;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.SymbolicExecution;
using CSharpExplodedGraph = SonarAnalyzer.SymbolicExecution.CSharpExplodedGraph;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(S2583DiagnosticId)]
    [Rule(S2589DiagnosticId)]
    public sealed class ConditionEvaluatesToConstant : SonarDiagnosticAnalyzer
    {
        private static readonly ISet<SyntaxKind> OmittedSyntaxKinds = new HashSet<SyntaxKind>
        {
            SyntaxKind.LogicalAndExpression,
            SyntaxKind.LogicalOrExpression
        };

        private static readonly ISet<SyntaxKind> LoopBreakingStatements = new HashSet<SyntaxKind>
        {
            SyntaxKind.BreakStatement,
            SyntaxKind.ThrowStatement,
            SyntaxKind.ReturnStatement
        };

        private static readonly ISet<SyntaxKind> LoopStatements = new HashSet<SyntaxKind>
        {
            SyntaxKind.WhileStatement,
            SyntaxKind.DoStatement,
            SyntaxKind.ForStatement
        };

        private static readonly ISet<SyntaxKind> ConditionalStatements = new HashSet<SyntaxKind>
        {
            SyntaxKind.IfStatement,
            SyntaxKind.WhileStatement,
            SyntaxKind.DoStatement,
            SyntaxKind.ConditionalExpression,
            SyntaxKind.CoalesceExpression
        };

        // Do not report in finally and catch blocks to avoid False Positives. To correctly solve
        // this problem we would need to link all CFG blocks for catch clauses to all statements within
        // the try block. This is unreasonable because it will generate tons of paths, thus making
        // the debugging a hell and probably slowing down the performance.
        private static readonly ISet<SyntaxKind> IgnoredBlocks = new HashSet<SyntaxKind>
        {
            SyntaxKind.FinallyClause,
            SyntaxKind.CatchClause,
        };

        private static readonly ISet<SyntaxKind> BooleanLiterals = new HashSet<SyntaxKind>
        {
            SyntaxKind.TrueLiteralExpression,
            SyntaxKind.FalseLiteralExpression
        };

        private const string S2583DiagnosticId = "S2583"; // Bug
        private const string S2583MessageFormat = "Change this condition so that it does not always evaluate to '{0}'; some subsequent code is never executed.";

        private const string S2589DiagnosticId = "S2589"; // Code smell
        private const string S2589MessageFormat = "Change this condition so that it does not always evaluate to '{0}'.";

        private static readonly DiagnosticDescriptor s2583 =
            DiagnosticDescriptorBuilder.GetDescriptor(S2583DiagnosticId, S2583MessageFormat, RspecStrings.ResourceManager);

        private static readonly DiagnosticDescriptor s2589 =
            DiagnosticDescriptorBuilder.GetDescriptor(S2589DiagnosticId, S2589MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(s2583, s2589);

        protected override void Initialize(SonarAnalysisContext context)
        {
            //FIXME: Temporary silence for CFG defork
            //context.RegisterExplodedGraphBasedAnalysis((e, c) => CheckForRedundantConditions(e, c));
        }

        private static void CheckForRedundantConditions(CSharpExplodedGraph explodedGraph, SyntaxNodeAnalysisContext context)
        {
            var conditionTrue = new HashSet<SyntaxNode>();
            var conditionFalse = new HashSet<SyntaxNode>();
            var hasYieldStatement = false;

            void instructionProcessed(object sender, InstructionProcessedEventArgs args) {
                hasYieldStatement = hasYieldStatement || IsYieldNode(args.ProgramPoint.Block);
            }

            void collectConditions(object sender, ConditionEvaluatedEventArgs args) =>
                CollectConditions(args, conditionTrue, conditionFalse, context.SemanticModel);

            void explorationEnded(object sender, EventArgs args) {
                // Do not raise issue in generator functions (See #1295)
                if (hasYieldStatement)
                {
                    return;
                }
                Enumerable.Empty<Diagnostic>()
                    .Union(conditionTrue
                        .Except(conditionFalse)
                        .Where(c => !IsConditionOfLoopWithBreak((ExpressionSyntax)c))
                        .Where(c => !IsInsideCatchOrFinallyBlock(c))
                        .Select(node => GetDiagnostics(node, true)))
                    .Union(conditionFalse
                        .Except(conditionTrue)
                        .Where(c => !IsInsideCatchOrFinallyBlock(c))
                        .Select(node => GetDiagnostics(node, false)))
                    .ToList()
                    .ForEach(d => context.ReportDiagnosticWhenActive(d));
            }

            explodedGraph.InstructionProcessed += instructionProcessed;
            explodedGraph.ExplorationEnded += explorationEnded;
            explodedGraph.ConditionEvaluated += collectConditions;

            try
            {
                explodedGraph.Walk();
            }
            finally
            {
                explodedGraph.InstructionProcessed -= instructionProcessed;
                explodedGraph.ExplorationEnded -= explorationEnded;
                explodedGraph.ConditionEvaluated -= collectConditions;
            }
        }

        private static bool IsYieldNode(Block node) =>
            node is JumpBlock jumpBlock &&
            jumpBlock.JumpNode.IsAnyKind(SyntaxKind.YieldReturnStatement, SyntaxKind.YieldBreakStatement);

        private static bool IsInsideCatchOrFinallyBlock(SyntaxNode c) =>
            c.Ancestors().Any(n => n.IsAnyKind(IgnoredBlocks));

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
                yield return unreachableExpressions.First().CreateLocation(unreachableExpressions.Last());
            }
            else
            {
                // When the constant expression is the only child of a if/while/etc. statement
                // it is not reported as unreachable, but regardless we want to check if its
                // parent has an unreachable branch.
                if (constantExpression is ExpressionSyntax expression)
                {
                    bool unreachableCodeIsDetectable = constantValue ?
                    !expression.GetSelfOrTopParenthesizedExpression().Parent.IsKind(SyntaxKind.LogicalAndExpression)
                    : !expression.GetSelfOrTopParenthesizedExpression().Parent.IsKind(SyntaxKind.LogicalOrExpression);
                    // when an expression is true with "And" parent or is false with "Or" parent
                    // we don't have enough information to know if we have an unreachable code
                    if (unreachableCodeIsDetectable)
                    {
                        unreachableExpressions.Add(expression);
                    }
                }
            }

            var statement = GetUnreachableStatement(unreachableExpressions, constantValue);
            if (statement != null)
            {
                yield return statement.GetLocation();
            }
        }

        private static SyntaxNode GetUnreachableStatement(IEnumerable<ExpressionSyntax> unreachableExpressions, bool constantValue)
        {
            var parent = unreachableExpressions
                .Select(CSharpSyntaxHelper.GetSelfOrTopParenthesizedExpression) // unreachable expressions
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

        private static void CollectConditions(ConditionEvaluatedEventArgs args, HashSet<SyntaxNode> conditionTrue, HashSet<SyntaxNode> conditionFalse, SemanticModel semanticModel)
        {
            var condition = (args.Condition as ExpressionSyntax).RemoveParentheses() ?? args.Condition;

            if (condition == null ||
                OmittedSyntaxKinds.Contains(condition.Kind()) ||
                IsConstantOrLiteralCondition(condition, semanticModel))
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

        private static bool IsConstantOrLiteralCondition(SyntaxNode condition, SemanticModel semanticModel)
        {
            if (!condition.Parent.IsAnyKind(ConditionalStatements))
            {
                return false;
            }

            return IsBooleanLiteral(condition) ||
                IsBooleanConstant(condition, semanticModel);

            bool IsBooleanConstant(SyntaxNode syntaxNode, SemanticModel model)
            {
                if (syntaxNode is MemberAccessExpressionSyntax ||
                    syntaxNode is IdentifierNameSyntax)
                {
                    var constant = semanticModel.GetConstantValue(syntaxNode);
                    return constant.HasValue && constant.Value is bool;
                }
                return false;
            }

            bool IsBooleanLiteral(SyntaxNode syntaxNode)
            {
                return syntaxNode.IsAnyKind(BooleanLiterals);
            }
        }
    }
}
