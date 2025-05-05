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

using Microsoft.CodeAnalysis.Text;

namespace SonarAnalyzer.CSharp.Styling.Rules;

public abstract class IndentBase : StylingAnalyzer
{
    protected IndentBase(string id, string expressionName) : base(id, $$"""Indent this {{expressionName}} at line position {0}.""") { }

    protected void Verify(SonarSyntaxNodeReportingContext context, int expected, SyntaxToken token, SyntaxNode reportingLocationExpression)
    {
        if (token.Line() != token.GetPreviousToken().Line() // Raise only when the line starts with this token to avoid collisions with T0024, T0027, etc.
            && token.GetLocation().GetLineSpan().StartLinePosition.Character != expected)
        {
            context.ReportIssue(Rule, Location.Create(token.SyntaxTree, TextSpan.FromBounds(token.SpanStart, reportingLocationExpression.Span.End)), (expected + 1).ToString());
        }
    }

    protected static int? ExpectedPosition(SyntaxNode node) =>
        StatementRoot(node) is { } root
            ? (root.GetLocation().GetLineSpan().StartLinePosition.Character + 4) / 4 * 4    // Nearest next tab distance
            : null;

    private static SyntaxNode StatementRoot(SyntaxNode node)
    {
        var current = node;
        while (current is not null)
        {
            if (current is ForStatementSyntax)
            {
                return node;    // Root from the ternary condition itself
            }
            else if (current is StatementSyntax
                || (current is ExpressionSyntax && current.Parent is IfStatementSyntax or WhileStatementSyntax)
                || current.Parent is ArrowExpressionClauseSyntax or ArgumentSyntax or LambdaExpressionSyntax)
            {
                return current;
            }
            else
            {
                current = current.Parent;
            }
        }
        return null;
    }
}
