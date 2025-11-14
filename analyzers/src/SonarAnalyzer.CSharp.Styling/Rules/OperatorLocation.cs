/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
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
public class OperatorLocation : StylingAnalyzer
{
    public OperatorLocation() : base("T0018", "The '{0}' operator should not be at the end of the line.") { }

    protected override void Initialize(SonarAnalysisContext context)
    {
        context.RegisterNodeAction(c => Validate(c, ((BinaryExpressionSyntax)c.Node).OperatorToken),
            SyntaxKind.LogicalAndExpression,
            SyntaxKind.LogicalOrExpression,
            SyntaxKind.BitwiseAndExpression,
            SyntaxKind.BitwiseOrExpression,
            SyntaxKind.ExclusiveOrExpression,
            SyntaxKind.CoalesceExpression,
            SyntaxKind.AddExpression,
            SyntaxKind.LeftShiftExpression,
            SyntaxKind.RightShiftExpression,
            SyntaxKind.UnsignedRightShiftExpression,
            SyntaxKind.SubtractExpression,
            SyntaxKind.MultiplyExpression,
            SyntaxKind.DivideExpression,
            SyntaxKind.ModuloExpression,
            SyntaxKind.EqualsExpression,
            SyntaxKind.NotEqualsExpression,
            SyntaxKind.GreaterThanExpression,
            SyntaxKind.GreaterThanOrEqualExpression,
            SyntaxKind.LessThanExpression,
            SyntaxKind.LessThanOrEqualExpression,
            SyntaxKind.AsExpression,
            SyntaxKind.IsExpression);
        context.RegisterNodeAction(c => Validate(c, ((BinaryPatternSyntax)c.Node).OperatorToken),
            SyntaxKind.AndPattern,
            SyntaxKind.OrPattern);
        context.RegisterNodeAction(c => Validate(c, ((IsPatternExpressionSyntax)c.Node).IsKeyword),
            SyntaxKind.IsPatternExpression);
        context.RegisterNodeAction(c => Validate(c, ((RangeExpressionSyntax)c.Node).OperatorToken),
            SyntaxKind.RangeExpression);
        context.RegisterNodeAction(c =>
            {
                var conditional = (ConditionalExpressionSyntax)c.Node;
                Validate(c, conditional.QuestionToken);
                Validate(c, conditional.ColonToken);
            },
            SyntaxKind.ConditionalExpression);
        context.RegisterNodeAction(c => Validate(c, ((MemberAccessExpressionSyntax)c.Node).OperatorToken),
            SyntaxKind.SimpleMemberAccessExpression);
        context.RegisterNodeAction(c => Validate(c, ((MemberBindingExpressionSyntax)c.Node).OperatorToken),
            SyntaxKind.MemberBindingExpression);
        context.RegisterNodeAction(c => Validate(c, ((QualifiedNameSyntax)c.Node).DotToken),
            SyntaxKind.QualifiedName);
    }

    private void Validate(SonarSyntaxNodeReportingContext context, SyntaxToken token)
    {
        if (token.GetLocation().StartLine() != token.GetNextToken().GetLocation().StartLine())
        {
            context.ReportIssue(Rule, token, token.ToString());
        }
    }
}
