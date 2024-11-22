/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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

namespace SonarAnalyzer.VisualBasic.Core.Syntax.Extensions;

public static class ExpressionSyntaxExtensions
{
    private static readonly SyntaxKind[] LiteralSyntaxKinds =
        [
            SyntaxKind.CharacterLiteralExpression,
            SyntaxKind.FalseLiteralExpression,
            SyntaxKind.NothingLiteralExpression,
            SyntaxKind.NumericLiteralExpression,
            SyntaxKind.StringLiteralExpression,
            SyntaxKind.TrueLiteralExpression,
        ];

    public static ExpressionSyntax RemoveParentheses(this ExpressionSyntax expression) =>
        (ExpressionSyntax)SyntaxNodeExtensionsVisualBasic.RemoveParentheses(expression);

    public static ExpressionSyntax GetSelfOrTopParenthesizedExpression(this ExpressionSyntax expression) =>
        (ExpressionSyntax)SyntaxNodeExtensionsVisualBasic.GetSelfOrTopParenthesizedExpression(expression);

    public static bool NameIs(this ExpressionSyntax expression, string name) =>
        expression.GetName().Equals(name, StringComparison.InvariantCultureIgnoreCase);

    public static bool HasConstantValue(this ExpressionSyntax expression, SemanticModel semanticModel) =>
        expression.RemoveParentheses().IsAnyKind(LiteralSyntaxKinds) || expression.FindConstantValue(semanticModel) is not null;

    public static bool IsLeftSideOfAssignment(this ExpressionSyntax expression)
    {
        var topParenthesizedExpression = expression.GetSelfOrTopParenthesizedExpression();
        return topParenthesizedExpression.Parent.IsKind(SyntaxKind.SimpleAssignmentStatement) &&
            topParenthesizedExpression.Parent is AssignmentStatementSyntax assignment &&
            assignment.Left == topParenthesizedExpression;
    }

    public static bool IsOnBase(this ExpressionSyntax expression) =>
        IsOn(expression, SyntaxKind.MyBaseExpression);

    private static bool IsOn(this ExpressionSyntax expression, SyntaxKind onKind) =>
        expression switch
        {
            InvocationExpressionSyntax invocation => IsOn(invocation.Expression, onKind),
            // This is a simplification as we don't check where the method is defined (so this could be this or base)
            GlobalNameSyntax or GenericNameSyntax or IdentifierNameSyntax or QualifiedNameSyntax => true,
            MemberAccessExpressionSyntax memberAccessExpression when memberAccessExpression.IsKind(SyntaxKind.SimpleMemberAccessExpression) =>
                memberAccessExpression.Expression.RemoveParentheses().IsKind(onKind),
            ConditionalAccessExpressionSyntax conditionalAccess => conditionalAccess.Expression.RemoveParentheses().IsKind(onKind),
            _ => false,
        };
}
