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

namespace SonarAnalyzer.CSharp.Styling.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class IndentTernary : IndentBase
{
    public IndentTernary() : base("T0025", "ternary") { }

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(c =>
            {
                var ternary = (ConditionalExpressionSyntax)c.Node;
                if (ExpectedPosition(ternary) is { } expected)
                {
                    Verify(c, expected, ternary.QuestionToken, ternary.WhenTrue);
                    Verify(c, expected, ternary.ColonToken, ternary.WhenFalse);
                }
            },
            SyntaxKind.ConditionalExpression);

    protected override SyntaxNode NodeRoot(SyntaxNode node, SyntaxNode current) =>
        current == node && current.GetFirstToken().IsFirstTokenOnLine()
            ? current
            : base.NodeRoot(node, current);

    private int? ExpectedPosition(ConditionalExpressionSyntax ternary)
    {
        if (ternary.Condition is BinaryExpressionSyntax binary && binary.OperatorToken.IsFirstTokenOnLine())
        {
            return ExpectedPosition(binary.OperatorToken.GetLocation().GetLineSpan().StartLinePosition.Character, 4);
        }
        else if (ternary.Condition.GetLocation() is var location && location.StartLine() != location.EndLine())
        {
            return null;    // Unexpected multiline condition => not known, not supported to avoid FPs
        }
        else
        {
            return base.ExpectedPosition(ternary);
        }
    }
}
