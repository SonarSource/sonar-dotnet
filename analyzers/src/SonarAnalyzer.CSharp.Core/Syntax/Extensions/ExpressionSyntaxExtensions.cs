/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
 * mailto: contact AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

using SonarAnalyzer.CFG.Extensions;

namespace SonarAnalyzer.CSharp.Core.Syntax.Extensions;

public static class ExpressionSyntaxExtensions
{
    private static readonly SyntaxKind[] LiteralSyntaxKinds =
        [
            SyntaxKind.CharacterLiteralExpression,
            SyntaxKind.FalseLiteralExpression,
            SyntaxKind.NullLiteralExpression,
            SyntaxKind.NumericLiteralExpression,
            SyntaxKind.StringLiteralExpression,
            SyntaxKind.TrueLiteralExpression
        ];

    private static readonly HashSet<SyntaxKind> EqualsOrNotEquals =
        [
            SyntaxKind.EqualsExpression,
            SyntaxKind.NotEqualsExpression
        ];

    public static ExpressionSyntax RemoveParentheses(this ExpressionSyntax expression) =>
        (ExpressionSyntax)((SyntaxNode)expression).RemoveParentheses();

    public static bool CanBeNull(this ExpressionSyntax expression, SemanticModel semanticModel) =>
        semanticModel.GetTypeInfo(expression).Type is { } expressionType
        && (expressionType.IsReferenceType || expressionType.Is(KnownType.System_Nullable_T));

    public static ExpressionSyntax RemoveConditionalAccess(this ExpressionSyntax node)
    {
        var whenNotNull = node.RemoveParentheses();
        while (whenNotNull is ConditionalAccessExpressionSyntax conditionalAccess)
        {
            whenNotNull = conditionalAccess.WhenNotNull.RemoveParentheses();
        }
        return whenNotNull;
    }

    /// <summary>
    /// Given an expression:
    /// - verify if it contains a comparison against the 'null' literal.
    /// - if it does contain a comparison, extract the expression which is compared against 'null' and whether the comparison is affirmative or negative.
    /// </summary>
    /// <remarks>
    /// The caller is expected to verify unary negative expression before. For example, for "!(x == null)", the caller should extract what is inside the parenthesis.
    /// </remarks>
    /// <param name="expression">Expression expected to contain a comparison to null.</param>
    /// <param name="compared">The variable/expression which gets compared to null.</param>
    /// <param name="isAffirmative">True if the check is affirmative ("== null", "is null"), false if negative ("!= null", "is not null")</param>
    /// <returns>True if the expression contains a comparison to null.</returns>
    public static bool TryGetExpressionComparedToNull(this ExpressionSyntax expression, out ExpressionSyntax compared, out bool isAffirmative)
    {
        compared = null;
        isAffirmative = false;
        if (expression.RemoveParentheses() is BinaryExpressionSyntax binary && EqualsOrNotEquals.Contains(binary.Kind()))
        {
            isAffirmative = binary.IsKind(SyntaxKind.EqualsExpression);
            var binaryLeft = binary.Left.RemoveParentheses();
            var binaryRight = binary.Right.RemoveParentheses();
            if (CSharpEquivalenceChecker.AreEquivalent(binaryLeft, SyntaxConstants.NullLiteralExpression))
            {
                compared = binaryRight;
                return true;
            }
            else if (CSharpEquivalenceChecker.AreEquivalent(binaryRight, SyntaxConstants.NullLiteralExpression))
            {
                compared = binaryLeft;
                return true;
            }
        }

        if (IsPatternExpressionSyntaxWrapper.IsInstance(expression.RemoveParentheses()))
        {
            var isPatternWrapper = (IsPatternExpressionSyntaxWrapper)expression.RemoveParentheses();
            if (isPatternWrapper.IsNotNull())
            {
                isAffirmative = false;
                compared = isPatternWrapper.Expression;
                return true;
            }
            else if (isPatternWrapper.IsNull())
            {
                isAffirmative = true;
                compared = isPatternWrapper.Expression;
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Returns the expression, representing the left side of the dot. This is useful for finding the expression of an invoked expression. <br/>
    /// For the expression of the invocation <c>M()</c> in the expression <c>this.A.B.M()</c> the member access <c>this.A.B</c> is returned and <br/>
    /// for <c>this.A?.B?.M()</c> the member binding <c>.B</c> is returned.
    /// </summary>
    /// <param name="expression">The expression to start the search of. Should be an MemberAccess or a MemberBinding.</param>
    /// <returns>The expression left of the dot or question mark dot.</returns>
    public static ExpressionSyntax GetLeftOfDot(this ExpressionSyntax expression) =>
        expression switch
        {
            MemberAccessExpressionSyntax memberAccessExpression => memberAccessExpression.Expression,
            MemberBindingExpressionSyntax memberBindingExpression => memberBindingExpression.GetParentConditionalAccessExpression()?.Expression,
            _ => null,
        };

    public static ExpressionSyntax GetSelfOrTopParenthesizedExpression(this ExpressionSyntax node) =>
         (ExpressionSyntax)SyntaxNodeExtensionsCSharp.GetSelfOrTopParenthesizedExpression((SyntaxNode)node);

    public static bool IsOnThis(this ExpressionSyntax expression) =>
        IsOn(expression, SyntaxKind.ThisExpression);

    private static bool IsOn(this ExpressionSyntax expression, SyntaxKind onKind) =>
        expression switch
        {
            InvocationExpressionSyntax invocation => IsOn(invocation.Expression, onKind),
            // Following statement is a simplification as we don't check where the method is defined (so this could be this or base)
            AliasQualifiedNameSyntax or GenericNameSyntax or IdentifierNameSyntax or QualifiedNameSyntax => true,
            MemberAccessExpressionSyntax memberAccess => memberAccess.Expression.RemoveParentheses().IsKind(onKind),
            ConditionalAccessExpressionSyntax conditionalAccess => conditionalAccess.Expression.RemoveParentheses().IsKind(onKind),
            _ => false,
        };

    public static bool IsInNameOfArgument(this ExpressionSyntax expression, SemanticModel semanticModel)
    {
        var parentInvocation = expression.FirstAncestorOrSelf<InvocationExpressionSyntax>();
        return parentInvocation is not null && parentInvocation.IsNameof(semanticModel);
    }

    public static bool IsStringEmpty(this ExpressionSyntax expression, SemanticModel semanticModel)
    {
        if (!expression.IsKind(SyntaxKind.SimpleMemberAccessExpression) &&
            !expression.IsKind(SyntaxKind.PointerMemberAccessExpression))
        {
            return false;
        }

        var nameSymbolInfo = semanticModel.GetSymbolInfo(((MemberAccessExpressionSyntax)expression).Name);

        return nameSymbolInfo.Symbol is not null &&
               nameSymbolInfo.Symbol.IsInType(KnownType.System_String) &&
               nameSymbolInfo.Symbol.Name == nameof(string.Empty);
    }

    public static bool HasConstantValue(this ExpressionSyntax expression, SemanticModel semanticModel) =>
        expression.RemoveParentheses().IsAnyKind(LiteralSyntaxKinds) || expression.FindConstantValue(semanticModel) is not null;

    public static bool IsLeftSideOfAssignment(this ExpressionSyntax expression)
    {
        var topParenthesizedExpression = expression.GetSelfOrTopParenthesizedExpression();
        return topParenthesizedExpression.Parent.IsKind(SyntaxKind.SimpleAssignmentExpression)
               && topParenthesizedExpression.Parent is AssignmentExpressionSyntax assignment
               && assignment.Left == topParenthesizedExpression;
    }
}
