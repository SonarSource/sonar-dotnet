/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
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

    extension(ExpressionSyntax expression)
    {
        public ExpressionSyntax WithoutEnclosingParentheses =>
            (ExpressionSyntax)expression.RemoveParentheses();

        public bool CanBeNull(SemanticModel semanticModel) =>
            semanticModel.GetTypeInfo(expression).Type is { } expressionType
            && (expressionType.IsReferenceType || expressionType.Is(KnownType.System_Nullable_T));

        public ExpressionSyntax RemoveConditionalAccess()
        {
            var whenNotNull = expression.WithoutEnclosingParentheses;
            while (whenNotNull is ConditionalAccessExpressionSyntax conditionalAccess)
            {
                whenNotNull = conditionalAccess.WhenNotNull.WithoutEnclosingParentheses;
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
        /// <param name="compared">The variable/expression which gets compared to null.</param>
        /// <param name="isAffirmative">True if the check is affirmative ("== null", "is null"), false if negative ("!= null", "is not null")</param>
        /// <returns>True if the expression contains a comparison to null.</returns>
        public bool TryGetExpressionComparedToNull(out ExpressionSyntax compared, out bool isAffirmative)
        {
            compared = null;
            isAffirmative = false;
            if (expression.WithoutEnclosingParentheses is BinaryExpressionSyntax binary && EqualsOrNotEquals.Contains(binary.Kind()))
            {
                isAffirmative = binary.IsKind(SyntaxKind.EqualsExpression);
                var binaryLeft = binary.Left.WithoutEnclosingParentheses;
                var binaryRight = binary.Right.WithoutEnclosingParentheses;
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

            if (IsPatternExpressionSyntaxWrapper.IsInstance(expression.WithoutEnclosingParentheses))
            {
                var isPatternWrapper = (IsPatternExpressionSyntaxWrapper)expression.WithoutEnclosingParentheses;
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
        public ExpressionSyntax LeftOfDot =>
            expression switch
            {
                MemberAccessExpressionSyntax memberAccessExpression => memberAccessExpression.Expression,
                MemberBindingExpressionSyntax memberBindingExpression => memberBindingExpression.GetParentConditionalAccessExpression()?.Expression,
                _ => null,
            };

        public ExpressionSyntax SelfOrTopParenthesizedExpression =>
             (ExpressionSyntax)SyntaxNodeExtensionsCSharp.GetSelfOrTopParenthesizedExpression(expression);

        public bool IsOnThis =>
            expression.IsOn(SyntaxKind.ThisExpression);

        public bool IsInNameOfArgument(SemanticModel semanticModel)
        {
            var parentInvocation = expression.FirstAncestorOrSelf<InvocationExpressionSyntax>();
            return parentInvocation is not null && parentInvocation.IsNameof(semanticModel);
        }

        public bool IsStringEmpty(SemanticModel semanticModel)
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

        /// <param name="strict">If true, result derived from field initializers and parameter default values will be omitted. Use it when you need certainty about the value.</param>
        public bool HasConstantValue(SemanticModel model, bool strict = false) =>
            expression.WithoutEnclosingParentheses.IsAnyKind(LiteralSyntaxKinds) || expression.FindConstantValue(model, strict) is not null;

        public bool IsLeftSideOfAssignment
        {
            get
            {
                var topParenthesizedExpression = expression.SelfOrTopParenthesizedExpression;
                return topParenthesizedExpression.Parent.IsKind(SyntaxKind.SimpleAssignmentExpression)
                       && topParenthesizedExpression.Parent is AssignmentExpressionSyntax assignment
                       && assignment.Left == topParenthesizedExpression;
            }
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
        public IReadOnlyCollection<ExpressionSyntax> ExtractMemberIdentifier() =>
            expression switch
            {
                IdentifierNameSyntax identifier => [identifier], // {Prop} -> Prop
                MemberAccessExpressionSyntax memberAccess => [memberAccess], // {Prop.Something} -> Prop.Something
                InvocationExpressionSyntax { ArgumentList.Arguments: { } arguments } invocation =>
                    [invocation, .. arguments.SelectMany(x => x.Expression.ExtractMemberIdentifier())], // {Method(a)} -> Method(), a
                ElementAccessExpressionSyntax { ArgumentList.Arguments: { } arguments } elementAccess => [elementAccess, .. arguments.SelectMany(x => x.Expression.ExtractMemberIdentifier())],
                ConditionalAccessExpressionSyntax conditional => [conditional.GetRootConditionalAccessExpression()],
                AwaitExpressionSyntax { Expression: { } awaited } => awaited.ExtractMemberIdentifier(), // {await Prop} _> Prop
                PrefixUnaryExpressionSyntax { Operand: { } operand } => operand.ExtractMemberIdentifier(), // {++Prop} -> Prop
                PostfixUnaryExpressionSyntax { Operand: { } operand } => operand.ExtractMemberIdentifier(), // {Prop++} -> Prop
                CastExpressionSyntax { Expression: { } operand } => operand.ExtractMemberIdentifier(), // {(Type)Prop} -> Prop
                BinaryExpressionSyntax { RawKind: (int)SyntaxKind.AsExpression or (int)SyntaxKind.IsExpression, Left: { } left } =>
                    left.ExtractMemberIdentifier(), // {Prop as Type} or {Prop is Type} -> Prop, {Prop is Constant} is ignored
                BinaryExpressionSyntax { Left: { } left, Right: { } right } => [.. left.ExtractMemberIdentifier(), .. right.ExtractMemberIdentifier()], // {Prop1 + Prop2} -> Prop1, Prop2
                ParenthesizedExpressionSyntax { Expression: { } parenthesized } => parenthesized.ExtractMemberIdentifier(), // {(Prop)} -> Prop
                InterpolatedStringExpressionSyntax { Contents: { } contents } => [.. contents.OfType<InterpolationSyntax>().SelectMany(x => x.Expression.ExtractMemberIdentifier())],
                // {Cond ? WhenTrue : WhenFalse} -> Cond, WhenTrue, WhenFalse
                ConditionalExpressionSyntax { Condition: { } condition, WhenTrue: { } whenTrue, WhenFalse: { } whenFalse } =>
                    [.. condition.ExtractMemberIdentifier(), .. whenTrue.ExtractMemberIdentifier(), .. whenFalse.ExtractMemberIdentifier()],
                // {Prop switch { Arm1 => InArm1, _ => InDefault }} -> Prop, InArm1, InDefault
                { } switchExpression when SwitchExpressionSyntaxWrapper.IsInstance(switchExpression) && (SwitchExpressionSyntaxWrapper)switchExpression is { } wrapped =>
                    [.. wrapped.GoverningExpression.ExtractMemberIdentifier(), .. wrapped.Arms.SelectMany(x => x.Expression.ExtractMemberIdentifier())],
                { } isPattern when IsPatternExpressionSyntaxWrapper.IsInstance(isPattern) && (IsPatternExpressionSyntaxWrapper)isPattern is { } wrapped =>
                    wrapped.Expression.ExtractMemberIdentifier(),
                _ => [],
            };

        /// <summary>
        /// On member access like operations, like <c>a.b</c>, c>a.b()</c>, or <c>a[b]</c>, the most left hand
        /// member (<c>a</c>) is returned. <see langword="this"/> is skipped, so <c>this.a</c> returns <c>a</c>.
        /// </summary>
        public ExpressionSyntax LeftMostInMemberAccess =>
            expression switch
            {
                IdentifierNameSyntax identifier => identifier, // Prop
                MemberAccessExpressionSyntax { Expression: IdentifierNameSyntax identifier } => identifier, // Prop.Something -> Prop
                MemberAccessExpressionSyntax { Expression: ThisExpressionSyntax, Name: IdentifierNameSyntax identifier } => identifier, // this.Prop -> Prop
                MemberAccessExpressionSyntax { Expression: PredefinedTypeSyntax predefinedType } => predefinedType, // int.MaxValue -> int
                MemberAccessExpressionSyntax { Expression: { } left } => left.LeftMostInMemberAccess, // Prop.Something.Something -> Prop
                MemberBindingExpressionSyntax memberBindingExpression => memberBindingExpression.GetRootConditionalAccessExpression()?.Expression.LeftMostInMemberAccess, // C#14 a?.b -> a
                InvocationExpressionSyntax { Expression: { } left } => left.LeftMostInMemberAccess, // Method() -> Method, also this.Method() and Method().Something
                ElementAccessExpressionSyntax { Expression: { } left } => left.LeftMostInMemberAccess, // a[b] -> a
                ConditionalAccessExpressionSyntax conditional => conditional.GetRootConditionalAccessExpression().Expression.LeftMostInMemberAccess, // a?.b -> a
                ParenthesizedExpressionSyntax { Expression: { } inner } => inner.LeftMostInMemberAccess, // (a.b).c -> a
                PostfixUnaryExpressionSyntax { RawKind: (int)SyntaxKindEx.SuppressNullableWarningExpression } nullSuppression => nullSuppression.Operand.LeftMostInMemberAccess, // a! -> a
                ExpressionSyntax { RawKind: (int)SyntaxKindEx.FieldExpression } fieldExpression => fieldExpression, // C#14 field keyword
                _ => null,
            };

        /// <summary>
        /// Returns <see langword="true"/> if the expression is a default literal or a null supressed default literal, i.e. <c>default</c> or <c>default!</c>.
        /// It returns <see langword="false"/> for other expressions, including <c>default(object)</c> or <c>default(object)!</c>.
        /// </summary>
        public bool IsDefaultLiteral =>
            expression?.WithoutEnclosingParentheses is { } innerExpression
            && (innerExpression.IsKind(SyntaxKindEx.DefaultLiteralExpression)
                || (innerExpression is PostfixUnaryExpressionSyntax { RawKind: (int)SyntaxKindEx.SuppressNullableWarningExpression, Operand: { } supressionOperand }
                    && supressionOperand.WithoutEnclosingParentheses is { RawKind: (int)SyntaxKindEx.DefaultLiteralExpression }));

        private bool IsOn(SyntaxKind onKind) =>
            expression switch
            {
                InvocationExpressionSyntax invocation => invocation.Expression.IsOn(onKind),
                // Following statement is a simplification as we don't check where the method is defined (so this could be this or base)
                AliasQualifiedNameSyntax or GenericNameSyntax or IdentifierNameSyntax or QualifiedNameSyntax => true,
                MemberAccessExpressionSyntax memberAccess => memberAccess.Expression.WithoutEnclosingParentheses.IsKind(onKind),
                ConditionalAccessExpressionSyntax conditionalAccess => conditionalAccess.Expression.WithoutEnclosingParentheses.IsKind(onKind),
                _ => false,
            };
    }
}
