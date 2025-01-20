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

namespace SonarAnalyzer.Rules;

public abstract class RedundantParenthesesBase<TParenthesizedExpression, TSyntaxKind>
    : SonarDiagnosticAnalyzer
    where TParenthesizedExpression : SyntaxNode
    where TSyntaxKind : struct
{
    internal const string DiagnosticId = "S1110";
    protected const string MessageFormat = "Remove these redundant parentheses.";
    protected const string SecondaryMessage = "Remove the redundant closing parentheses.";

    protected abstract GeneratedCodeRecognizer GeneratedCodeRecognizer { get; }
    protected abstract TSyntaxKind ParenthesizedExpressionSyntaxKind { get; }

    protected abstract SyntaxNode GetExpression(TParenthesizedExpression parenthesizedExpression);
    protected abstract SyntaxToken GetOpenParenToken(TParenthesizedExpression parenthesizedExpression);
    protected abstract SyntaxToken GetCloseParenToken(TParenthesizedExpression parenthesizedExpression);

    protected sealed override void Initialize(SonarAnalysisContext context)
    {
        context.RegisterNodeAction(
            GeneratedCodeRecognizer,
            c =>
            {
                var expression = (TParenthesizedExpression)c.Node;

                if (!(expression.Parent is TParenthesizedExpression) &&
                    (GetExpression(expression) is TParenthesizedExpression))
                {
                    var innermostExpression = GetSelfAndDescendantParenthesizedExpressions(expression)
                        .Reverse()
                        .Skip(1)
                        .First(); // There are always at least two parenthesized expressions
                    var location = GetOpenParenToken(expression).CreateLocation(GetOpenParenToken(innermostExpression));
                    var secondaryLocation = GetCloseParenToken(innermostExpression).CreateLocation(GetCloseParenToken(expression)).ToSecondary(SecondaryMessage);
                    c.ReportIssue(SupportedDiagnostics[0], location, [secondaryLocation]);
                }
            },
            ParenthesizedExpressionSyntaxKind);
    }

    protected IEnumerable<TParenthesizedExpression> GetSelfAndDescendantParenthesizedExpressions(TParenthesizedExpression expression)
    {
        var descendant = expression;
        while (descendant is not null)
        {
            yield return descendant;
            descendant = GetExpression(descendant) as TParenthesizedExpression;
        }
    }
}
