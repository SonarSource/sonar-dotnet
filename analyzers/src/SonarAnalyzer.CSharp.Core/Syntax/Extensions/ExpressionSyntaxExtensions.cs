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

using SonarAnalyzer.CFG.Extensions;

namespace SonarAnalyzer.CSharp.Core.Syntax.Extensions;

public static class ExpressionSyntaxExtensions
{
    private static readonly HashSet<SyntaxKind> LiteralSyntaxKinds =
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

    /// <summary>
    /// Extracts all <see cref="SyntaxNode"/> that can be bound to fields, methods, parameters, locals and so on from the <paramref name="expression"/>.
    /// <list type="table">
    /// <item><c>a + b</c> returns <c>[IdentifierName(a), IdentifierName(b)]</c></item>
    /// <item><c>a.b</c> returns <c>[MemberAccess(a, b)]</c></item>
    /// <item><c>a is b</c> returns <c>[IdentifierName(a)]</c></item>. Note: <c>b</c> can bind to a type or a constant member. The constant member bind is ignored.
    /// <item><c>(b)a</c> returns <c>[IdentifierName(a)]</c></item>. Note: b can only bind to a type and is not returned.
    /// <item><c>a is { b.c: d e }</c> returns <c>[IdentifierName(a)]</c></item> Patterns are not visited.
    /// <item><c>a switch { b c => d }</c> returns <c>[IdentifierName(a), IdentifierName(d)]</c></item> Patterns are not visited.
    /// </list>
    /// </summary>
    public static IReadOnlyCollection<ExpressionSyntax> ExtractMemberIdentifier(this ExpressionSyntax expression) =>
        expression switch
        {
            IdentifierNameSyntax identifier => [identifier], // {Prop} -> Prop
            MemberAccessExpressionSyntax memberAccess => [memberAccess], // {Prop.Something} -> Prop.Something
            InvocationExpressionSyntax { ArgumentList.Arguments: { } arguments } invocation =>
                [invocation, .. arguments.SelectMany(x => ExtractMemberIdentifier(x.Expression))], // {Method(a)} -> Method(), a
            ElementAccessExpressionSyntax { ArgumentList.Arguments: { } arguments } elementAccess => [elementAccess, .. arguments.SelectMany(x => ExtractMemberIdentifier(x.Expression))],
            ConditionalAccessExpressionSyntax conditional => [conditional.GetRootConditionalAccessExpression()],
            AwaitExpressionSyntax { Expression: { } awaited } => ExtractMemberIdentifier(awaited), // {await Prop} _> Prop
            PrefixUnaryExpressionSyntax { Operand: { } operand } => ExtractMemberIdentifier(operand), // {++Prop} -> Prop
            PostfixUnaryExpressionSyntax { Operand: { } operand } => ExtractMemberIdentifier(operand), // {Prop++} -> Prop
            CastExpressionSyntax { Expression: { } operand } => ExtractMemberIdentifier(operand), // {(Type)Prop} -> Prop
            BinaryExpressionSyntax { RawKind: (int)SyntaxKind.AsExpression or (int)SyntaxKind.IsExpression, Left: { } left } =>
                ExtractMemberIdentifier(left), // {Prop as Type} or {Prop is Type} -> Prop, {Prop is Constant} is ignored
            BinaryExpressionSyntax { Left: { } left, Right: { } right } => [.. ExtractMemberIdentifier(left), .. ExtractMemberIdentifier(right)], // {Prop1 + Prop2} -> Prop1, Prop2
            ParenthesizedExpressionSyntax { Expression: { } parenthesized } => ExtractMemberIdentifier(parenthesized), // {(Prop)} -> Prop
            InterpolatedStringExpressionSyntax { Contents: { } contents } => [.. contents.OfType<InterpolationSyntax>().SelectMany(x => ExtractMemberIdentifier(x.Expression))],
            // {Cond ? WhenTrue : WhenFalse} -> Cond, WhenTrue, WhenFalse
            ConditionalExpressionSyntax { Condition: { } condition, WhenTrue: { } whenTrue, WhenFalse: { } whenFalse } =>
                [.. ExtractMemberIdentifier(condition), .. ExtractMemberIdentifier(whenTrue), .. ExtractMemberIdentifier(whenFalse)],
            // {Prop switch { Arm1 => InArm1, _ => InDefault }} -> Prop, InArm1, InDefault
            { } switchExpression when SwitchExpressionSyntaxWrapper.IsInstance(switchExpression) && (SwitchExpressionSyntaxWrapper)switchExpression is { } wrapped =>
                [.. ExtractMemberIdentifier(wrapped.GoverningExpression), .. wrapped.Arms.SelectMany(x => ExtractMemberIdentifier(x.Expression))],
            { } isPattern when IsPatternExpressionSyntaxWrapper.IsInstance(isPattern) && (IsPatternExpressionSyntaxWrapper)isPattern is { } wrapped =>
                ExtractMemberIdentifier(wrapped.Expression),
            _ => [],
        };

    /// <summary>
    /// On member access like operations, like <c>a.b</c>, c>a.b()</c>, or <c>a[b]</c>, the most left hand
    /// member (<c>a</c>) is returned. <see langword="this"/> is skipped, so <c>this.a</c> returns <c>a</c>.
    /// </summary>
    public static ExpressionSyntax GetLeftMostInMemberAccess(this ExpressionSyntax expression) => expression switch
    {
        IdentifierNameSyntax identifier => identifier, // Prop
        MemberAccessExpressionSyntax { Expression: IdentifierNameSyntax identifier } => identifier, // Prop.Something -> Prop
        MemberAccessExpressionSyntax { Expression: ThisExpressionSyntax, Name: IdentifierNameSyntax identifier } => identifier, // this.Prop -> Prop
        MemberAccessExpressionSyntax { Expression: PredefinedTypeSyntax predefinedType } => predefinedType, // int.MaxValue -> int
        MemberAccessExpressionSyntax { Expression: { } left } => GetLeftMostInMemberAccess(left), // Prop.Something.Something -> Prop
        InvocationExpressionSyntax { Expression: { } left } => GetLeftMostInMemberAccess(left), // Method() -> Method, also this.Method() and Method().Something
        ElementAccessExpressionSyntax { Expression: { } left } => GetLeftMostInMemberAccess(left), // a[b] -> a
        ConditionalAccessExpressionSyntax conditional => GetLeftMostInMemberAccess(conditional.GetRootConditionalAccessExpression().Expression), // a?.b -> a
        ParenthesizedExpressionSyntax { Expression: { } inner } => GetLeftMostInMemberAccess(inner), // (a.b).c -> a
        PostfixUnaryExpressionSyntax { RawKind: (int)SyntaxKindEx.SuppressNullableWarningExpression } nullSuppression => GetLeftMostInMemberAccess(nullSuppression.Operand), // a! -> a
        _ => null,
    };

    /// <summary>
    /// Returns <see langword="true"/> if the expression is a default literal or a null supressed default literal, i.e. <c>default</c> or <c>default!</c>.
    /// It returns <see langword="false"/> for other expressions, including <c>default(object)</c> or <c>default(object)!</c>.
    /// </summary>
    /// <param name="expression">The potential default literal.</param>
    /// <returns>Returns <see langword="true"/> if the expressions is a default literal or a supressed null literal.</returns>
    public static bool IsDefaultLiteral(this ExpressionSyntax expression) =>
        expression?.RemoveParentheses() is { } innerExpression
        && (innerExpression.IsKind(SyntaxKindEx.DefaultLiteralExpression)
            || (innerExpression is PostfixUnaryExpressionSyntax { RawKind: (int)SyntaxKindEx.SuppressNullableWarningExpression, Operand: { } supressionOperand }
                && supressionOperand.RemoveParentheses() is { RawKind: (int)SyntaxKindEx.DefaultLiteralExpression }));

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
}
