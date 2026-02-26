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

using Comparison = SonarAnalyzer.Core.Syntax.Utilities.ComparisonKind;

namespace SonarAnalyzer.CSharp.Rules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class DoNotCheckZeroSizeCollection : DoNotCheckZeroSizeCollectionBase<SyntaxKind>
    {
        protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;
        protected override string IEnumerableTString => "IEnumerable<T>";

        protected override void Initialize(SonarAnalysisContext context)
        {
            base.Initialize(context);
            context.RegisterNodeAction(AnalyzeIsPatternExpression, SyntaxKindEx.IsPatternExpression);
            context.RegisterNodeAction(AnalyzeSwitchExpression, SyntaxKindEx.SwitchExpression);
            context.RegisterNodeAction(AnalyzeSwitchStatement, SyntaxKind.SwitchStatement);
            context.RegisterNodeAction(AnalyzePropertyPatternClause, SyntaxKindEx.PropertyPatternClause);
        }

        private void AnalyzePropertyPatternClause(SonarSyntaxNodeReportingContext c)
        {
            var propertyPatternClause = (PropertyPatternClauseSyntaxWrapper)c.Node;

            foreach (var subPattern in propertyPatternClause.Subpatterns)
            {
                if (subPattern.ExpressionColon.SyntaxNode is NameColonSyntax nameColon)
                {
                    CheckPatternCondition(c, nameColon.Name, subPattern.Pattern.SyntaxNode.RemoveParentheses());
                }
                else if (ExpressionColonSyntaxWrapper.IsInstance(subPattern.ExpressionColon.SyntaxNode)
                         && (ExpressionColonSyntaxWrapper)subPattern.ExpressionColon.SyntaxNode is var expressionColon)
                {
                    CheckPatternCondition(c, expressionColon.Expression, subPattern.Pattern.SyntaxNode.RemoveParentheses());
                }
            }
        }

        private void AnalyzePatterns(SonarSyntaxNodeReportingContext c, ExpressionSyntax expression, SyntaxNode pattern)
        {
            foreach (var pair in expression.MapToPattern(pattern))
            {
                CheckPatternCondition(c, pair.Key, pair.Value);
            }
        }

        private void AnalyzeIsPatternExpression(SonarSyntaxNodeReportingContext c)
        {
            var isPatternExpression = (IsPatternExpressionSyntaxWrapper)c.Node;
            AnalyzePatterns(c, isPatternExpression.Expression, isPatternExpression.Pattern);
        }

        private void AnalyzeSwitchExpression(SonarSyntaxNodeReportingContext c)
        {
            var switchExpression = (SwitchExpressionSyntaxWrapper)c.Node;

            foreach (var arm in switchExpression.Arms)
            {
                AnalyzePatterns(c, switchExpression.GoverningExpression, arm.Pattern.SyntaxNode);
            }
        }

        private void AnalyzeSwitchStatement(SonarSyntaxNodeReportingContext c)
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

        private void CheckPatternCondition(SonarSyntaxNodeReportingContext context, SyntaxNode expression, SyntaxNode pattern)
        {
            if (pattern.DescendantNodesAndSelf().FirstOrDefault(x => x.IsKind(SyntaxKindEx.RelationalPattern)) is { } relationalPatternNode
                && ((RelationalPatternSyntaxWrapper)relationalPatternNode) is var relationalPattern
                && relationalPattern.OperatorToken.ToComparisonKind() is { } comparison
                && comparison != Comparison.None
                && Language.ExpressionNumericConverter.ConstantIntValue(context.Model, relationalPattern.Expression) is { } constant)
            {
                CheckExpression(context, relationalPattern.SyntaxNode, expression, constant, comparison);
            }
        }
    }
}
