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
public sealed class TernaryLine : StylingAnalyzer
{
    public TernaryLine() : base("T0024", "Place branches of the multiline ternary on a separate line.") { }

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(c =>
            {
                var ternary = (ConditionalExpressionSyntax)c.Node;
                var location = ternary.GetLocation();
                if (location.StartLine() != location.EndLine())
                {
                    Verify(c, ternary.QuestionToken, ternary.WhenTrue);
                    Verify(c, ternary.ColonToken, ternary.WhenFalse);
                }
            },
            SyntaxKind.ConditionalExpression);

    private void Verify(SonarSyntaxNodeReportingContext context, SyntaxToken token, ExpressionSyntax expression)
    {
        if (token.Line() == token.GetPreviousToken().Line())
        {
            context.ReportIssue(Rule, Location.Create(expression.SyntaxTree, TextSpan.FromBounds(token.SpanStart, expression.Span.End)));
        }
    }
}
