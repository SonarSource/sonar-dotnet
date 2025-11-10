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

using Microsoft.CodeAnalysis.Text;

namespace SonarAnalyzer.CSharp.Styling.Rules;

public abstract class IndentBase : StylingAnalyzer
{
    protected IndentBase(string id, string expressionName) : base(id, $$"""Indent this {{expressionName}} at line position {0}.""") { }

    protected void Verify(SonarSyntaxNodeReportingContext context, int expected, SyntaxToken token, SyntaxNode reportingLocationExpression = null)
    {
        if (token.IsFirstTokenOnLine() // Raise only when the line starts with this token to avoid collisions with T0024, T0027, etc.
            && token.GetLocation().GetLineSpan().StartLinePosition.Character != expected)
        {
            context.ReportIssue(Rule, Location.Create(token.SyntaxTree, TextSpan.FromBounds(token.SpanStart, (reportingLocationExpression?.Span ?? token.Span).End)), (expected + 1).ToString());
        }
    }

    protected virtual int Offset(SyntaxNode node, SyntaxNode root) =>
        4;

    protected int? ExpectedPosition(SyntaxNode node) =>
        StatementRoot(node) is { } root
            ? ExpectedPosition(root.GetLocation().GetLineSpan().StartLinePosition.Character, Offset(node, root))
            : null;

    protected static int ExpectedPosition(int character, int offset) =>
        (character + offset) / 4 * 4;   // Nearest next tab distance

    protected virtual SyntaxNode NodeRoot(SyntaxNode node, SyntaxNode current)
    {
        if (current is ForStatementSyntax)
        {
            return node;    // Root from the original node itself (ternary, binary, ...)
        }
        else if (current is StatementSyntax or AssignmentExpressionSyntax or SwitchExpressionArmSyntax
            || current is ExpressionSyntax { Parent: IfStatementSyntax or WhileStatementSyntax }
            || current.Parent is ArrowExpressionClauseSyntax or LambdaExpressionSyntax
            || (current is InvocationExpressionSyntax or CollectionExpressionSyntax && current.GetFirstToken().IsFirstTokenOnLine()))
        {
            return current;
        }
        else
        {
            return null;
        }
    }

    protected virtual bool IsIgnored(SyntaxNode node) =>
        false;

    private SyntaxNode StatementRoot(SyntaxNode node)
    {
        var current = node;
        while (current is not null)
        {
            if (IsIgnored(current))
            {
                return null;
            }
            else if (NodeRoot(node, current) is { } result)
            {
                return result;
            }
            else
            {
                current = current.Parent;
            }
        }
        return null;
    }
}
