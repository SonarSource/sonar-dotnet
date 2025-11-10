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
public sealed class UsePositiveLogic : StylingAnalyzer
{
    public UsePositiveLogic() : base("T0033", "Swap the branches and use positive condition.") { }

    protected override void Initialize(SonarAnalysisContext context)
    {
        context.RegisterNodeAction(c =>
            {
                var ifStatement = (IfStatementSyntax)c.Node;
                if (ifStatement.Else is { Statement: not IfStatementSyntax })
                {
                    Validate(c, ifStatement.Condition);
                }
            },
            SyntaxKind.IfStatement);

        context.RegisterNodeAction(c =>
            Validate(c, ((ConditionalExpressionSyntax)c.Node).Condition),
            SyntaxKind.ConditionalExpression);
    }

    private void Validate(SonarSyntaxNodeReportingContext context, ExpressionSyntax expression)
    {
        if (IsNegative(expression, null))
        {
            context.ReportIssue(Rule, expression);
        }
    }

    private static bool IsNegative(ExpressionSyntax expression, Operator outerAndOr) =>
        expression switch
        {
            PrefixUnaryExpressionSyntax prefix => prefix.IsKind(SyntaxKind.LogicalNotExpression),
            BinaryExpressionSyntax binary => IsNegative(binary, outerAndOr),
            ParenthesizedExpressionSyntax parenthesized => IsNegative(parenthesized.Expression, outerAndOr),
            IsPatternExpressionSyntax isPattern => IsNegative(isPattern.Pattern, outerAndOr),
            _ => false
        };

    private static bool IsNegative(BinaryExpressionSyntax binary, Operator outerAndOr) =>
        binary.IsKind(SyntaxKind.NotEqualsExpression)
        || (CheckAndOr(binary, outerAndOr) is { } currentandOr && IsNegative(binary.Left, currentandOr) && IsNegative(binary.Right, currentandOr));

    private static bool IsNegative(PatternSyntax pattern, Operator outerAndOr) =>
        pattern is UnaryPatternSyntax { RawKind: (int)SyntaxKind.NotPattern }
        || (pattern is BinaryPatternSyntax { } binary && CheckAndOr(binary, outerAndOr) is { } currentAndOr && IsNegative(binary.Left, currentAndOr) && IsNegative(binary.Right, currentAndOr));

    private static Operator CheckAndOr(SyntaxNode node, Operator outerAndOr) =>
        (outerAndOr ?? Operator.From((SyntaxKind)node.RawKind)) is { } currentAndOr && currentAndOr.IsMatch((SyntaxKind)node.RawKind)
            ? currentAndOr
            : null;

    private sealed class Operator
    {
        private static readonly Operator And = new(SyntaxKind.LogicalAndExpression, SyntaxKind.AndPattern);
        private static readonly Operator Or = new(SyntaxKind.LogicalOrExpression, SyntaxKind.OrPattern);

        private readonly SyntaxKind logical;
        private readonly SyntaxKind pattern;

        private Operator(SyntaxKind logical, SyntaxKind pattern)
        {
            this.logical = logical;
            this.pattern = pattern;
        }

        public static Operator From(SyntaxKind kind)
        {
            if (kind == And.logical || kind == And.pattern)
            {
                return And;
            }
            else if (kind == Or.logical || kind == Or.pattern)
            {
                return Or;
            }
            else
            {
                return null;
            }
        }

        public bool IsMatch(SyntaxKind kind) =>
            kind == logical || kind == pattern;
    }
}
