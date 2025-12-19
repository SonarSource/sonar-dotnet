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

namespace SonarAnalyzer.CSharp.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AssignmentInsideSubExpression : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S1121";
    private const string MessageFormat = "Extract the assignment of '{0}' from this expression.";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    private static readonly HashSet<SyntaxKind> AllowedParentExpressionKinds =
    [
        SyntaxKind.SimpleAssignmentExpression,
        SyntaxKind.ParenthesizedLambdaExpression,
        SyntaxKind.SimpleLambdaExpression,
        SyntaxKind.AnonymousMethodExpression,
    ];

    private static readonly HashSet<SyntaxKind> RelationalExpressionKinds =
    [
        SyntaxKind.EqualsExpression,
        SyntaxKind.NotEqualsExpression,
        SyntaxKind.LessThanExpression,
        SyntaxKind.LessThanOrEqualExpression,
        SyntaxKind.GreaterThanExpression,
        SyntaxKind.GreaterThanOrEqualExpression,
        SyntaxKindEx.IsPatternExpression
    ];

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(
            c =>
            {
                var assignment = (AssignmentExpressionSyntax)c.Node;
                var topParenthesizedExpression = assignment.GetSelfOrTopParenthesizedExpression();
                if (IsNonCompliantSubExpression(assignment, topParenthesizedExpression) || IsDirectlyInStatementCondition(assignment, topParenthesizedExpression))
                {
                    c.ReportIssue(Rule, assignment.OperatorToken, assignment.Left.ToString());
                }
            },
            SyntaxKind.SimpleAssignmentExpression,
            SyntaxKind.AddAssignmentExpression,
            SyntaxKind.SubtractAssignmentExpression,
            SyntaxKind.MultiplyAssignmentExpression,
            SyntaxKind.DivideAssignmentExpression,
            SyntaxKind.ModuloAssignmentExpression,
            SyntaxKind.AndAssignmentExpression,
            SyntaxKind.ExclusiveOrAssignmentExpression,
            SyntaxKind.OrAssignmentExpression,
            SyntaxKind.LeftShiftAssignmentExpression,
            SyntaxKind.RightShiftAssignmentExpression,
            SyntaxKindEx.UnsignedRightShiftAssignmentExpression);

    private static bool IsNonCompliantSubExpression(AssignmentExpressionSyntax assignment, ExpressionSyntax topParenthesizedExpression) =>
        !IsInInitializerExpression(topParenthesizedExpression)
        && topParenthesizedExpression.Parent.FirstAncestorOrSelf<ExpressionSyntax>() is { } expressionParent
        && !IsCompliantAssignmentInsideExpression(assignment, expressionParent);

    private static bool IsInInitializerExpression(ExpressionSyntax expression) =>
        expression.Parent?.Kind() is SyntaxKindEx.WithInitializerExpression or SyntaxKind.ObjectInitializerExpression;

    private static bool IsCompliantAssignmentInsideExpression(AssignmentExpressionSyntax assignment, ExpressionSyntax expressionParent) =>
        IsCompliantCoalesceExpression(expressionParent, assignment)
        || (RelationalExpressionKinds.Contains(expressionParent.Kind()) && IsInStatementCondition(expressionParent))
        || (AllowedParentExpressionKinds.Contains(expressionParent.Kind()) && !IsInInitializerExpression(expressionParent));

    private static bool IsCompliantCoalesceExpression(ExpressionSyntax parentExpression, AssignmentExpressionSyntax assignment) =>
        assignment.IsKind(SyntaxKind.SimpleAssignmentExpression)
        && CoalesceExpressionParent(parentExpression) is { } coalesceExpression
        && CSharpEquivalenceChecker.AreEquivalent(assignment.Left.RemoveParentheses(), coalesceExpression.Left.RemoveParentheses());

    private static BinaryExpressionSyntax CoalesceExpressionParent(ExpressionSyntax parent) =>
        parent.AncestorsAndSelf()
            .TakeWhile(x => x is ExpressionSyntax)
            .OfType<BinaryExpressionSyntax>()
            .FirstOrDefault(x => x.IsKind(SyntaxKind.CoalesceExpression));

    private static bool IsDirectlyInStatementCondition(ExpressionSyntax expression, ExpressionSyntax topParenthesizedExpression)
    {
        return IsDirectlyInStatementCondition<IfStatementSyntax>(x => x.Condition)
            || IsDirectlyInStatementCondition<ForStatementSyntax>(x => x.Condition)
            || IsDirectlyInStatementCondition<WhileStatementSyntax>(x => x.Condition)
            || IsDirectlyInStatementCondition<DoStatementSyntax>(x => x.Condition);

        bool IsDirectlyInStatementCondition<T>(Func<T, ExpressionSyntax> conditionSelector) where T : SyntaxNode =>
            topParenthesizedExpression.Parent.FirstAncestorOrSelf<T>() is { } statement && conditionSelector(statement).RemoveParentheses() == expression;
    }

    private static bool IsInStatementCondition(ExpressionSyntax expression)
    {
        return expression.GetSelfOrTopParenthesizedExpression() is var expressionOrParenthesizedParent
            && (IsInStatementCondition<IfStatementSyntax>(x => x.Condition)
                || IsInStatementCondition<ForStatementSyntax>(x => x.Condition)
                || IsInStatementCondition<WhileStatementSyntax>(x => x.Condition)
                || IsInStatementCondition<DoStatementSyntax>(x => x.Condition));

        bool IsInStatementCondition<T>(Func<T, ExpressionSyntax> conditionSelector) where T : SyntaxNode =>
            expressionOrParenthesizedParent.Parent.FirstAncestorOrSelf<T>() is { } statement && conditionSelector(statement) is { } condition && condition.Contains(expression);
    }
}
