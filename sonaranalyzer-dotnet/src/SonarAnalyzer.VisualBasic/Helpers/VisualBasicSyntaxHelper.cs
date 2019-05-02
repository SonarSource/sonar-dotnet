/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace SonarAnalyzer.Helpers.VisualBasic
{
    internal static class VisualBasicSyntaxHelper
    {
        private static readonly SyntaxKind[] LiteralSyntaxKinds =
            new[]
            {
                SyntaxKind.CharacterLiteralExpression,
                SyntaxKind.FalseLiteralExpression,
                SyntaxKind.NothingLiteralExpression,
                SyntaxKind.NumericLiteralExpression,
                SyntaxKind.StringLiteralExpression,
                SyntaxKind.TrueLiteralExpression,
            };

        public static SyntaxNode RemoveParentheses(this SyntaxNode expression)
        {
            var currentExpression = expression;
            while (currentExpression?.IsKind(SyntaxKind.ParenthesizedExpression) ?? false)
            {
                currentExpression = ((ParenthesizedExpressionSyntax)currentExpression).Expression;
            }
            return currentExpression;
        }

        public static ExpressionSyntax RemoveParentheses(this ExpressionSyntax expression) =>
            (ExpressionSyntax)RemoveParentheses((SyntaxNode)expression);

        public static SyntaxNode GetSelfOrTopParenthesizedExpression(this SyntaxNode node)
        {
            var current = node;
            while (current?.Parent?.IsKind(SyntaxKind.ParenthesizedExpression) ?? false)
            {
                current = current.Parent;
            }
            return current;
        }

        public static ExpressionSyntax GetSelfOrTopParenthesizedExpression(this ExpressionSyntax expression) =>
            (ExpressionSyntax)GetSelfOrTopParenthesizedExpression((SyntaxNode)expression);

        public static SyntaxNode GetFirstNonParenthesizedParent(this SyntaxNode node) =>
            node.GetSelfOrTopParenthesizedExpression().Parent;

        #region Statement

        public static StatementSyntax GetPrecedingStatement(this StatementSyntax currentStatement)
        {
            var children = currentStatement.Parent.ChildNodes().ToList();
            var index = children.IndexOf(currentStatement);
            return index == 0 ? null : children[index - 1] as StatementSyntax;
        }

        public static StatementSyntax GetSucceedingStatement(this StatementSyntax currentStatement)
        {
            var children = currentStatement.Parent.ChildNodes().ToList();
            var index = children.IndexOf(currentStatement);
            return index == children.Count - 1 ? null : children[index + 1] as StatementSyntax;
        }

        #endregion Statement

        public static bool IsAnyKind(this SyntaxNode syntaxNode, params SyntaxKind[] syntaxKinds) =>
           syntaxNode != null && syntaxKinds.Contains((SyntaxKind)syntaxNode.RawKind);

        public static bool IsAnyKind(this SyntaxToken syntaxToken, ISet<SyntaxKind> collection) =>
            collection.Contains((SyntaxKind)syntaxToken.RawKind);

        public static bool IsAnyKind(this SyntaxNode syntaxNode, ISet<SyntaxKind> collection) =>
            syntaxNode != null && collection.Contains((SyntaxKind)syntaxNode.RawKind);

        public static bool AnyOfKind(this IEnumerable<SyntaxNode> nodes, SyntaxKind kind) =>
            nodes.Any(n => n.RawKind == (int)kind);

        public static bool AnyOfKind(this IEnumerable<SyntaxToken> tokens, SyntaxKind kind) =>
            tokens.Any(n => n.RawKind == (int)kind);

        public static SyntaxToken? GetMethodCallIdentifier(this InvocationExpressionSyntax invocation)
        {
           if (invocation == null)
            {
                return null;
            }
            var expressionType = invocation.Expression.Kind();
            // in vb.net when using the null - conditional operator (e.g.handle?.IsClosed), the parser
            // will generate a SimpleMemberAccessExpression and not a MemberBindingExpressionSyntax like for C#
            switch (expressionType)
            {
                case SyntaxKind.IdentifierName:
                    return ((IdentifierNameSyntax)invocation.Expression).Identifier;
                case SyntaxKind.SimpleMemberAccessExpression:
                    return ((MemberAccessExpressionSyntax)invocation.Expression).Name.Identifier;
                default:
                    return null;
            }
        }
        public static bool IsMethodInvocation(this InvocationExpressionSyntax expression, KnownType type, string methodName, SemanticModel semanticModel) =>
            semanticModel.GetSymbolInfo(expression).Symbol is IMethodSymbol methodSymbol &&
            methodSymbol.IsInType(type) &&
            // vbnet is case insensitive
            methodName.Equals(methodSymbol.Name, System.StringComparison.InvariantCultureIgnoreCase);

        public static bool IsOnThis(this ExpressionSyntax expression) =>
            IsOn(expression, SyntaxKind.MeExpression);

        public static bool IsOnBase(this ExpressionSyntax expression) =>
            IsOn(expression, SyntaxKind.MyBaseExpression);

        private static bool IsOn(this ExpressionSyntax expression, SyntaxKind onKind)
        {
            switch (expression.Kind())
            {
                case SyntaxKind.InvocationExpression:
                    return IsOn(((InvocationExpressionSyntax)expression).Expression, onKind);

                case SyntaxKind.GlobalName:
                case SyntaxKind.GenericName:
                case SyntaxKind.IdentifierName:
                case SyntaxKind.QualifiedName:
                    // This is a simplification as we don't check where the method is defined (so this could be this or base)
                    return true;

                case SyntaxKind.SimpleMemberAccessExpression:
                    return ((MemberAccessExpressionSyntax)expression).Expression.RemoveParentheses().IsKind(onKind);

                case SyntaxKind.ConditionalAccessExpression:
                    return ((ConditionalAccessExpressionSyntax)expression).Expression.RemoveParentheses().IsKind(onKind);

                default:
                    return false;
            }
        }

        public static SimpleNameSyntax GetIdentifier(this ExpressionSyntax expression)
        {
            switch (expression?.Kind())
            {
                case SyntaxKind.SimpleMemberAccessExpression:
                    return ((MemberAccessExpressionSyntax)expression).Name;
                case SyntaxKind.IdentifierName:
                    return (IdentifierNameSyntax)expression;
                default:
                    return null;
            }
        }

        public static bool IsConstant(this ExpressionSyntax expression, SemanticModel semanticModel)
        {
            if (expression == null)
            {
                return false;
            }
            return expression.RemoveParentheses().IsAnyKind(LiteralSyntaxKinds) ||
                semanticModel.GetConstantValue(expression).HasValue;
        }

        public static string GetStringValue(this SyntaxNode node) =>
            node != null &&
            node.IsKind(SyntaxKind.StringLiteralExpression) &&
            node is LiteralExpressionSyntax literal
                ? literal.Token.ValueText
                : null;

        public static bool IsLeftSideOfAssignment(this ExpressionSyntax expression)
        {
            var topParenthesizedExpression = expression.GetSelfOrTopParenthesizedExpression();
            return topParenthesizedExpression.Parent.IsKind(SyntaxKind.SimpleAssignmentStatement) &&
                topParenthesizedExpression.Parent is AssignmentStatementSyntax assignment &&
                assignment.Left == topParenthesizedExpression;
        }

        public static bool IsComment(this SyntaxTrivia trivia)
        {
            switch (trivia.Kind())
            {
                case SyntaxKind.CommentTrivia:
                case SyntaxKind.DocumentationCommentExteriorTrivia:
                case SyntaxKind.DocumentationCommentTrivia:
                    return true;

                default:
                    return false;
            }
        }

        public static Location FindIdentifierLocation(this MethodBlockBaseSyntax methodBlockBase) =>
            GetIdentifierOrDefault(methodBlockBase)?.GetLocation();

        public static SyntaxToken? GetIdentifierOrDefault(this MethodBlockBaseSyntax methodBlockBase)
        {
            var blockStatement = methodBlockBase?.BlockStatement;

            switch (blockStatement?.Kind())
            {
                case SyntaxKind.SubNewStatement:
                    return (blockStatement as SubNewStatementSyntax)?.NewKeyword;

                case SyntaxKind.FunctionStatement:
                case SyntaxKind.SubStatement:
                    return (blockStatement as MethodStatementSyntax)?.Identifier;

                default:
                    return null;
            }
        }

        public static ExpressionSyntax Get(this ArgumentListSyntax argumentList, int index) =>
            argumentList != null && argumentList.Arguments.Count > index
                ? argumentList.Arguments[index].GetExpression().RemoveParentheses()
                : null;
    }
}
