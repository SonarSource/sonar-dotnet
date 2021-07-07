/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Extensions;
using SonarAnalyzer.Helpers;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class DoNotCheckZeroSizeCollection : DoNotCheckZeroSizeCollectionBase<SyntaxKind, BinaryExpressionSyntax, ExpressionSyntax>
    {
        protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;
        protected override SyntaxKind GreaterThanOrEqualExpression => SyntaxKind.GreaterThanOrEqualExpression;
        protected override SyntaxKind LessThanOrEqualExpression => SyntaxKind.LessThanOrEqualExpression;
        protected override string IEnumerableTString { get; } = "IEnumerable<T>";

        protected override void Initialize(SonarAnalysisContext context)
        {
            base.Initialize(context);
            context.RegisterSyntaxNodeActionInNonGenerated(AnalyzeIsPatternExpression, SyntaxKindEx.IsPatternExpression);
            context.RegisterSyntaxNodeActionInNonGenerated(AnalyzeSwitchExpression, SyntaxKindEx.SwitchExpression);
            context.RegisterSyntaxNodeActionInNonGenerated(AnalyzeSwitchStatement, SyntaxKind.SwitchStatement);
            context.RegisterSyntaxNodeActionInNonGenerated(AnalyzePropertyPatternClause, SyntaxKindEx.PropertyPatternClause);
        }

        private void AnalyzePropertyPatternClause(SyntaxNodeAnalysisContext c)
        {
            var propertyPatternClause = (PropertyPatternClauseSyntaxWrapper)c.Node;

            foreach (var subPattern in propertyPatternClause.Subpatterns)
            {
                CheckPatternCondition(c, subPattern.NameColon.Name, subPattern.Pattern.SyntaxNode.RemoveParentheses());
            }
        }

        private void AnalyzePatterns(SyntaxNodeAnalysisContext c, ExpressionSyntax expression, SyntaxNode pattern)
        {
            var objectToPatternMap = new Dictionary<ExpressionSyntax, SyntaxNode>();
            MapObjectToPattern(expression, pattern, objectToPatternMap);

            foreach (var exp in objectToPatternMap.Keys)
            {
                CheckPatternCondition(c, exp, objectToPatternMap[exp]);
            }
        }

        private void AnalyzeIsPatternExpression(SyntaxNodeAnalysisContext c)
        {
            var isPatternExpression = (IsPatternExpressionSyntaxWrapper)c.Node;
            AnalyzePatterns(c, isPatternExpression.Expression, isPatternExpression.Pattern);
        }

        private void AnalyzeSwitchExpression(SyntaxNodeAnalysisContext c)
        {
            var switchExpression = (SwitchExpressionSyntaxWrapper)c.Node;

            foreach (var arm in switchExpression.Arms)
            {
                AnalyzePatterns(c, switchExpression.GoverningExpression, arm.Pattern.SyntaxNode);
            }
        }

        private void AnalyzeSwitchStatement(SyntaxNodeAnalysisContext c)
        {
            var switchStatement = (SwitchStatementSyntax)c.Node;
            foreach (var section in switchStatement.Sections)
            {
                foreach (var label in section.Labels)
                {
                    AnalyzePatterns(c, switchStatement.Expression, label);
                }
            }
        }

        private void CheckPatternCondition(SyntaxNodeAnalysisContext context, ExpressionSyntax expression, SyntaxNode pattern)
        {
            var relationalOrSubPattern =  pattern.DescendantNodesAndSelf().FirstOrDefault(x => x.IsAnyKind(SyntaxKindEx.RelationalPattern, SyntaxKindEx.Subpattern));
            if (RelationalPatternSyntaxWrapper.IsInstance(relationalOrSubPattern)
                && ((RelationalPatternSyntaxWrapper)relationalOrSubPattern) is var relationalPattern
                && relationalPattern.OperatorToken.ValueText.Equals(">="))
            {
                CheckCondition(context, relationalPattern, expression, relationalPattern.Expression);
            }
        }

        protected override ExpressionSyntax GetLeftNode(BinaryExpressionSyntax binaryExpression) =>
            binaryExpression.Left;

        protected override ExpressionSyntax GetRightNode(BinaryExpressionSyntax binaryExpression) =>
            binaryExpression.Right;

        protected override ExpressionSyntax RemoveParentheses(ExpressionSyntax expression) =>
            expression.RemoveParentheses();

        protected override ISymbol GetSymbol(SyntaxNodeAnalysisContext context, ExpressionSyntax expression)
        {
            while (expression is ConditionalAccessExpressionSyntax conditionalAccess)
            {
                expression = conditionalAccess.WhenNotNull;
            }

            return context.SemanticModel.GetSymbolInfo(expression).Symbol;
        }

        private void MapObjectToPattern(ExpressionSyntax expression, SyntaxNode pattern, IDictionary<ExpressionSyntax, SyntaxNode> objectToPatternMap)
        {
            var expressionWithoutParenthesis = expression.RemoveParentheses();
            var patternWithoutParenthesis = pattern.RemoveParentheses();

            if (TupleExpressionSyntaxWrapper.IsInstance(expressionWithoutParenthesis)
                && ((TupleExpressionSyntaxWrapper)expressionWithoutParenthesis) is var tupleExpression)
            {
                if (!RecursivePatternSyntaxWrapper.IsInstance(patternWithoutParenthesis)
                    || ((RecursivePatternSyntaxWrapper)patternWithoutParenthesis is { } recursivePattern
                        && recursivePattern.PositionalPatternClause.SyntaxNode == null))
                {
                    return;
                }

                if (recursivePattern.PositionalPatternClause.Subpatterns.Count != tupleExpression.Arguments.Count)
                {
                    return;
                }

                for (var i = 0; i < tupleExpression.Arguments.Count; i++)
                {
                    MapObjectToPattern(tupleExpression.Arguments[i].Expression, recursivePattern.PositionalPatternClause.Subpatterns[i].Pattern, objectToPatternMap);
                }
            }
            else
            {
                objectToPatternMap.Add(expressionWithoutParenthesis, patternWithoutParenthesis);
            }
        }
    }
}
