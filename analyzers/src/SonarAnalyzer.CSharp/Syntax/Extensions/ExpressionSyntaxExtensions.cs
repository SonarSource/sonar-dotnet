/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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

namespace SonarAnalyzer.CSharp.Syntax.Extensions;

internal static class ExpressionSyntaxExtensions
{
    /// <summary>
    /// Maps the tuple arguments of <paramref name="expression"/> to the positional sub-pattern of <paramref name="pattern"/>.
    /// For a pattern like: <code>(x, y) is (1, 2)</code>x is mapped to numeric literal 1 and y is mapped to 2.
    /// </summary>
    /// <param name="expression">A tuple expression.</param>
    /// <param name="pattern">A pattern that can be matched to the tuple <paramref name="expression"/>.</param>
    /// <param name="objectToPatternMap">The mapping between the tuple arguments and the positional sub-patterns.</param>
    public static Dictionary<ExpressionSyntax, SyntaxNode> MapToPattern(this ExpressionSyntax expression, SyntaxNode pattern)
    {
        var map = new Dictionary<ExpressionSyntax, SyntaxNode>();
        FillPatternMap(map, expression, pattern);
        return map;
    }

    private static void FillPatternMap(Dictionary<ExpressionSyntax, SyntaxNode> map, ExpressionSyntax expression, SyntaxNode pattern)
    {
        expression = expression.RemoveParentheses();
        pattern = pattern.RemoveParentheses();

        if (TupleExpressionSyntaxWrapper.IsInstance(expression)
            && (TupleExpressionSyntaxWrapper)expression is var tupleExpression
            && RecursivePatternSyntaxWrapper.IsInstance(pattern)
            && (RecursivePatternSyntaxWrapper)pattern is var recursivePattern
            && recursivePattern.PositionalPatternClause.SyntaxNode is not null
            && recursivePattern.PositionalPatternClause.Subpatterns.Count == tupleExpression.Arguments.Count)
        {
            for (var i = 0; i < tupleExpression.Arguments.Count; i++)
            {
                FillPatternMap(map, tupleExpression.Arguments[i].Expression, recursivePattern.PositionalPatternClause.Subpatterns[i].Pattern);
            }
        }
        else
        {
            map.Add(expression, pattern);
        }
    }
}
