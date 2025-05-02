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

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class IndentTernary : StylingAnalyzer
{
    public IndentTernary() : base("T0025", "Indent this ternary at line position {0}.") { }

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(c =>
            {
                var ternary = (ConditionalExpressionSyntax)c.Node;
                var location = ternary.GetLocation();
                if (location.StartLine() != location.EndLine() && StatementRoot(ternary) is { } root)
                {
                    var expected = (root.GetLocation().GetLineSpan().StartLinePosition.Character + 4) / 4 * 4; // Nearest next tab distance
                    Verify(c, expected, ternary.QuestionToken, ternary.WhenTrue);
                    Verify(c, expected, ternary.ColonToken, ternary.WhenFalse);
                }
            },
            SyntaxKind.ConditionalExpression);

    private void Verify(SonarSyntaxNodeReportingContext context, int expected, SyntaxToken token, ExpressionSyntax expression)
    {
        if (token.Line() != token.GetPreviousToken().Line() // Don't raise in T0024 cases
            && token.GetLocation().GetLineSpan().StartLinePosition.Character != expected)
        {
            context.ReportIssue(Rule, Location.Create(expression.SyntaxTree, TextSpan.FromBounds(token.SpanStart, expression.Span.End)), (expected + 1).ToString());
        }
    }

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
