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

namespace SonarAnalyzer.VisualBasic.Core.Syntax.Extensions;

public static class ExpressionSyntaxExtensions
{
    private static readonly HashSet<SyntaxKind> LiteralSyntaxKinds =
        [
            SyntaxKind.CharacterLiteralExpression,
            SyntaxKind.FalseLiteralExpression,
            SyntaxKind.NothingLiteralExpression,
            SyntaxKind.NumericLiteralExpression,
            SyntaxKind.StringLiteralExpression,
            SyntaxKind.TrueLiteralExpression,
        ];

    extension(ExpressionSyntax expression)
    {
        public ExpressionSyntax SelfOrTopParenthesizedExpression => (ExpressionSyntax)SyntaxNodeExtensionsVisualBasic.GetSelfOrTopParenthesizedExpression(expression);

        public bool IsOnBase => expression.IsOn(SyntaxKind.MyBaseExpression);

        /// <summary>
        /// On member access like operations, like <c>a.b</c>, c>a.b()</c>, or <c>a[b]</c>, the most left hand
        /// member (<c>a</c>) is returned. <see langword="Me"/> is skipped, so <c>this.a</c> returns <c>a</c>.
        /// </summary>
        public ExpressionSyntax LeftMostInMemberAccess =>
            expression switch
            {
                IdentifierNameSyntax identifier => identifier,                                                                          // Prop
                MemberAccessExpressionSyntax { Expression: IdentifierNameSyntax identifier } => identifier,                             // Prop.Something -> Prop
                MemberAccessExpressionSyntax { Expression: MeExpressionSyntax, Name: IdentifierNameSyntax identifier } => identifier,   // this.Prop -> Prop
                MemberAccessExpressionSyntax { Expression: PredefinedTypeSyntax predefinedType } => predefinedType,                     // int.MaxValue -> int
                MemberAccessExpressionSyntax { Expression: { } left } => left.LeftMostInMemberAccess,                                   // Prop.Something.Something -> Prop
                InvocationExpressionSyntax { Expression: { } left } => left.LeftMostInMemberAccess,                                     // Method() -> Method, also this.Method() and Method().Something
                ConditionalAccessExpressionSyntax conditional => conditional.RootConditionalAccessExpression.Expression.LeftMostInMemberAccess, // a?.b -> a
                ParenthesizedExpressionSyntax { Expression: { } inner } => inner.LeftMostInMemberAccess,                                // (a.b).c -> a
                _ => null,
            };

        public ConditionalAccessExpressionSyntax RootConditionalAccessExpression =>
            expression.AncestorsAndSelf().TakeWhile(x => x is ExpressionSyntax).OfType<ConditionalAccessExpressionSyntax>().LastOrDefault();

        public bool IsLeftSideOfAssignment
        {
            get
            {
                var topParenthesizedExpression = expression.SelfOrTopParenthesizedExpression;
                return topParenthesizedExpression.Parent.IsKind(SyntaxKind.SimpleAssignmentStatement)
                    && topParenthesizedExpression.Parent is AssignmentStatementSyntax assignment
                    && assignment.Left == topParenthesizedExpression;
            }
        }

        public ExpressionSyntax RemoveParentheses() =>
            (ExpressionSyntax)SyntaxNodeExtensionsVisualBasic.RemoveParentheses(expression);

        public bool NameIs(string name) =>
            expression.GetName().Equals(name, StringComparison.InvariantCultureIgnoreCase);

        public bool HasConstantValue(SemanticModel model) =>
            expression.RemoveParentheses().IsAnyKind(LiteralSyntaxKinds) || expression.FindConstantValue(model) is not null;

        private bool IsOn(SyntaxKind onKind) =>
            expression switch
            {
                InvocationExpressionSyntax invocation => invocation.Expression.IsOn(onKind),
                // This is a simplification as we don't check where the method is defined (so this could be this or base)
                GlobalNameSyntax or GenericNameSyntax or IdentifierNameSyntax or QualifiedNameSyntax => true,
                MemberAccessExpressionSyntax memberAccessExpression when memberAccessExpression.IsKind(SyntaxKind.SimpleMemberAccessExpression) =>
                    memberAccessExpression.Expression.RemoveParentheses().IsKind(onKind),
                ConditionalAccessExpressionSyntax conditionalAccess => conditionalAccess.Expression.RemoveParentheses().IsKind(onKind),
                _ => false,
            };
    }
}
