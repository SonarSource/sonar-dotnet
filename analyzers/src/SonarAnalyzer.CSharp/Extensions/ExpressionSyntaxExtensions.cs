/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

namespace SonarAnalyzer.Extensions
{
    public static class ExpressionSyntaxExtensions
    {
        private static readonly ISet<SyntaxKind> EqualsOrNotEquals = new HashSet<SyntaxKind>
        {
            SyntaxKind.EqualsExpression,
            SyntaxKind.NotEqualsExpression
        };

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
                if (CSharpEquivalenceChecker.AreEquivalent(binaryLeft, CSharpSyntaxHelper.NullLiteralExpression))
                {
                    compared = binaryRight;
                    return true;
                }
                else if (CSharpEquivalenceChecker.AreEquivalent(binaryRight, CSharpSyntaxHelper.NullLiteralExpression))
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

        // Copied from
        // https://github.com/dotnet/roslyn/blob/575bc42589145ba18b4f1cc2267d02695f861d8f/src/Workspaces/SharedUtilitiesAndExtensions/Compiler/CSharp/Extensions/ExpressionSyntaxExtensions.cs#L319
        public static bool IsWrittenTo(
            this ExpressionSyntax expression,
            SemanticModel semanticModel,
            CancellationToken cancellationToken)
        {
            if (expression == null)
                return false;

            expression = GetExpressionToAnalyzeForWrites(expression);

            if (expression.IsOnlyWrittenTo())
                return true;

            if (expression.IsInRefContext(out var refParent))
            {
                // most cases of `ref x` will count as a potential write of `x`.  An important exception is:
                // `ref readonly y = ref x`.  In that case, because 'y' can't be written to, this would not
                // be a write of 'x'.
                if (refParent.Parent is EqualsValueClauseSyntax { Parent: VariableDeclaratorSyntax { Parent: VariableDeclarationSyntax { Type: { } variableDeclarationType } } })
                {
                    if (ScopedTypeSyntaxWrapper.IsInstance(variableDeclarationType) && (ScopedTypeSyntaxWrapper)variableDeclarationType is { } scopedType)
                    {
                        variableDeclarationType = scopedType.Type;
                    }

                    if (RefTypeSyntaxWrapper.IsInstance(variableDeclarationType) && ((RefTypeSyntaxWrapper)variableDeclarationType).ReadOnlyKeyword != default)
                    {
                        return false;
                    }
                }

                return true;
            }

            // Similar to `ref x`, `&x` allows reads and write of the value, meaning `x` may be (but is not definitely)
            // written to.
            if (expression.Parent.IsKind(SyntaxKind.AddressOfExpression))
                return true;

            // We're written if we're used in a ++, or -- expression.
            if (expression.IsOperandOfIncrementOrDecrementExpression())
                return true;

            if (expression.IsLeftSideOfAnyAssignExpression())
                return true;

            // An extension method invocation with a ref-this parameter can write to an expression.
            if (expression.Parent is MemberAccessExpressionSyntax memberAccess &&
                expression == memberAccess.Expression)
            {
                var symbol = semanticModel.GetSymbolInfo(memberAccess, cancellationToken).Symbol;
                if (symbol is IMethodSymbol
                    {
                        MethodKind: MethodKind.ReducedExtension,
                        ReducedFrom.Parameters: { Length: > 0 } parameters,
                    } && parameters[0].RefKind == RefKind.Ref)
                {
                    return true;
                }
            }

            return false;
        }

        private static ExpressionSyntax GetExpressionToAnalyzeForWrites(ExpressionSyntax? expression)
        {
            if (expression.IsRightSideOfDotOrArrow())
            {
                expression = (ExpressionSyntax)expression.Parent;
            }

            expression = (ExpressionSyntax)expression.WalkUpParentheses();

            return expression;
        }

        // Copy of
        // https://github.com/dotnet/roslyn/blob/575bc42589145ba18b4f1cc2267d02695f861d8f/src/Workspaces/SharedUtilitiesAndExtensions/Compiler/CSharp/Extensions/ExpressionSyntaxExtensions.cs#L63
        public static bool IsRightSideOfDotOrArrow(this ExpressionSyntax name)
            => IsAnyMemberAccessExpressionName(name) || IsRightSideOfQualifiedName(name);

        // Copy of
        // https://github.com/dotnet/roslyn/blob/575bc42589145ba18b4f1cc2267d02695f861d8f/src/Workspaces/SharedUtilitiesAndExtensions/Compiler/CSharp/Extensions/ExpressionSyntaxExtensions.cs#L41
        public static bool IsAnyMemberAccessExpressionName(this ExpressionSyntax expression)
        {
            if (expression == null)
                return false;

            return expression == (expression.Parent as MemberAccessExpressionSyntax)?.Name ||
                expression.IsMemberBindingExpressionName();
        }

        // Copy of
        // https://github.com/dotnet/roslyn/blob/575bc42589145ba18b4f1cc2267d02695f861d8f/src/Workspaces/SharedUtilitiesAndExtensions/Compiler/CSharp/Extensions/ExpressionSyntaxExtensions.cs#L50
        public static bool IsMemberBindingExpressionName(this ExpressionSyntax expression)
            => expression?.Parent is MemberBindingExpressionSyntax memberBinding &&
               memberBinding.Name == expression;

        // Copy of
        // https://github.com/dotnet/roslyn/blob/575bc42589145ba18b4f1cc2267d02695f861d8f/src/Workspaces/SharedUtilitiesAndExtensions/Compiler/CSharp/Extensions/ExpressionSyntaxExtensions.cs#L54
        public static bool IsRightSideOfQualifiedName(this ExpressionSyntax expression)
            => expression?.Parent is QualifiedNameSyntax qualifiedName && qualifiedName.Right == expression;

        // Copy of
        // https://github.com/dotnet/roslyn/blob/575bc42589145ba18b4f1cc2267d02695f861d8f/src/Workspaces/SharedUtilitiesAndExtensions/Compiler/CSharp/Extensions/ExpressionSyntaxExtensions.cs#L233
        public static bool IsOnlyWrittenTo(this ExpressionSyntax expression)
        {
            expression = GetExpressionToAnalyzeForWrites(expression);

            if (expression != null)
            {
                if (expression.IsInOutContext())
                {
                    return true;
                }

                if (expression.Parent != null)
                {
                    if (expression.IsLeftSideOfAssignExpression())
                    {
                        return true;
                    }

                    if (expression.IsAttributeNamedArgumentIdentifier())
                    {
                        return true;
                    }
                }

                if (IsExpressionOfArgumentInDeconstruction(expression))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// If this declaration or identifier is part of a deconstruction, find the deconstruction.
        /// If found, returns either an assignment expression or a foreach variable statement.
        /// Returns null otherwise.
        ///
        /// copied from SyntaxExtensions.GetContainingDeconstruction.
        /// </summary>
        // Copy of
        // https://github.com/dotnet/roslyn/blob/575bc42589145ba18b4f1cc2267d02695f861d8f/src/Workspaces/SharedUtilitiesAndExtensions/Compiler/CSharp/Extensions/ExpressionSyntaxExtensions.cs#L273
        private static bool IsExpressionOfArgumentInDeconstruction(ExpressionSyntax expr)
        {
            if (!expr.IsParentKind(SyntaxKind.Argument))
            {
                return false;
            }

            while (true)
            {
                var parent = expr.Parent;
                if (parent == null)
                {
                    return false;
                }

                switch (parent.Kind())
                {
                    case SyntaxKind.Argument:
                        if (parent.Parent?.Kind() == SyntaxKindEx.TupleExpression)
                        {
                            expr = (ExpressionSyntax)parent.Parent;
                            continue;
                        }

                        return false;
                    case SyntaxKind.SimpleAssignmentExpression:
                        if (((AssignmentExpressionSyntax)parent).Left == expr)
                        {
                            return true;
                        }

                        return false;
                    case SyntaxKindEx.ForEachVariableStatement:
                        if (((ForEachVariableStatementSyntaxWrapper)parent).Variable == expr)
                        {
                            return true;
                        }

                        return false;

                    default:
                        return false;
                }
            }
        }

        // Copy of
        // https://github.com/dotnet/roslyn/blob/575bc42589145ba18b4f1cc2267d02695f861d8f/src/Workspaces/SharedUtilitiesAndExtensions/Compiler/CSharp/Extensions/ExpressionSyntaxExtensions.cs#L190
        public static bool IsInOutContext(this ExpressionSyntax expression)
            => expression?.Parent is ArgumentSyntax { RefOrOutKeyword: SyntaxToken { RawKind: (int)SyntaxKind.OutKeyword } } argument &&
               argument.Expression == expression;

        // Copy of
        // https://github.com/dotnet/roslyn/blob/575bc42589145ba18b4f1cc2267d02695f861d8f/src/Workspaces/SharedUtilitiesAndExtensions/Compiler/CSharp/Extensions/ExpressionSyntaxExtensions.cs#L383
        public static bool IsAttributeNamedArgumentIdentifier(this ExpressionSyntax expression)
        {
            var nameEquals = expression?.Parent as NameEqualsSyntax;
            return nameEquals.IsParentKind(SyntaxKind.AttributeArgument);
        }

        public static bool IsInRefContext(this ExpressionSyntax expression)
            => IsInRefContext(expression, out _);

        /// <summary>
        /// Returns true if this expression is in some <c>ref</c> keyword context.  If <see langword="true"/> then
        /// <paramref name="refParent"/> will be the node containing the <see langword="ref"/> keyword.
        /// </summary>
        // Copy of
        // https://github.com/dotnet/roslyn/blob/575bc42589145ba18b4f1cc2267d02695f861d8f/src/Workspaces/SharedUtilitiesAndExtensions/Compiler/CSharp/Extensions/ExpressionSyntaxExtensions.cs#L201
        public static bool IsInRefContext(this ExpressionSyntax expression, out SyntaxNode refParent)
        {
            while (expression?.Parent is ParenthesizedExpressionSyntax or PostfixUnaryExpressionSyntax { RawKind: (int)SyntaxKindEx.SuppressNullableWarningExpression })
                expression = (ExpressionSyntax)expression.Parent;

            if (expression?.Parent switch
                {
                    ArgumentSyntax { RefOrOutKeyword.RawKind: (int)SyntaxKind.RefKeyword } => true,
                    var x when RefExpressionSyntaxWrapper.IsInstance(x) => true,
                    _ => false,
                })
            {
                refParent = expression.Parent;
                return true;
            }

            refParent = null;
            return false;
        }

        // Copy of
        // https://github.com/dotnet/roslyn/blob/575bc42589145ba18b4f1cc2267d02695f861d8f/src/Workspaces/SharedUtilitiesAndExtensions/Compiler/CSharp/Extensions/ExpressionSyntaxExtensions.cs#L389
        public static bool IsOperandOfIncrementOrDecrementExpression(this ExpressionSyntax expression)
        {
            if (expression?.Parent is SyntaxNode parent)
            {
                switch (parent.Kind())
                {
                    case SyntaxKind.PostIncrementExpression:
                    case SyntaxKind.PreIncrementExpression:
                    case SyntaxKind.PostDecrementExpression:
                    case SyntaxKind.PreDecrementExpression:
                        return true;
                }
            }

            return false;
        }
    }
}
