/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2017 SonarSource SA
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

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SonarAnalyzer.Helpers
{
    internal static class SyntaxHelper
    {
        public static readonly ExpressionSyntax NullLiteralExpression =
            SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression);
        public static readonly ExpressionSyntax FalseLiteralExpression =
            SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression);
        public static readonly ExpressionSyntax TrueLiteralExpression =
            SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression);
        public static readonly string NameOfKeywordText = SyntaxFacts.GetText(SyntaxKind.NameOfKeyword);

        public static bool HasExactlyNArguments(this InvocationExpressionSyntax invocation, int count)
        {
            return invocation != null &&
                invocation.ArgumentList != null &&
                invocation.ArgumentList.Arguments.Count == count;
        }

        public static ExpressionSyntax RemoveParentheses(this ExpressionSyntax expression)
        {
            var currentExpression = expression;
            var parentheses = expression as ParenthesizedExpressionSyntax;
            while (parentheses != null)
            {
                currentExpression = parentheses.Expression;
                parentheses = currentExpression as ParenthesizedExpressionSyntax;
            }
            return currentExpression;
        }

        public static ExpressionSyntax GetSelfOrTopParenthesizedExpression(this ExpressionSyntax node)
        {
            var current = node;
            var parent = current.Parent as ParenthesizedExpressionSyntax;
            while (parent != null)
            {
                current = parent;
                parent = current.Parent as ParenthesizedExpressionSyntax;
            }
            return current;
        }

        public static bool TryGetAttribute(this SyntaxList<AttributeListSyntax> attributeLists,
            KnownType attributeKnownType, SemanticModel semanticModel, out AttributeSyntax searchedAttribute)
        {
            searchedAttribute = null;

            if (!attributeLists.Any())
            {
                return false;
            }

            foreach (var attribute in attributeLists.SelectMany(attributeList => attributeList.Attributes))
            {
                var attributeType = semanticModel.GetTypeInfo(attribute).Type;

                if (attributeType.Is(attributeKnownType))
                {
                    searchedAttribute = attribute;
                    return true;
                }
            }

            return false;
        }

        public static bool IsOnThis(this ExpressionSyntax expression)
        {
            if (expression is NameSyntax)
            {
                return true;
            }

            var memberAccess = expression as MemberAccessExpressionSyntax;
            if (memberAccess != null &&
                memberAccess.Expression.RemoveParentheses().IsKind(SyntaxKind.ThisExpression))
            {
                return true;
            }

            var conditionalAccess = expression as ConditionalAccessExpressionSyntax;
            if (conditionalAccess != null &&
                conditionalAccess.Expression.RemoveParentheses().IsKind(SyntaxKind.ThisExpression))
            {
                return true;
            }

            return false;
        }

        public static bool IsInNameofCall(this ExpressionSyntax expression, SemanticModel semanticModel)
        {
            var argumentList = (expression.Parent as ArgumentSyntax)?.Parent as ArgumentListSyntax;
            var nameofCall = argumentList?.Parent as InvocationExpressionSyntax;

            if (nameofCall == null)
            {
                return false;
            }

            return nameofCall.IsNameof(semanticModel);
        }

        public static bool IsNameof(this InvocationExpressionSyntax expression, SemanticModel semanticModel)
        {
            var calledSymbol = semanticModel.GetSymbolInfo(expression).Symbol as IMethodSymbol;
            if (calledSymbol != null)
            {
                return false;
            }

            var nameofIdentifier = (expression?.Expression as IdentifierNameSyntax)?.Identifier;

            return nameofIdentifier.HasValue &&
                (nameofIdentifier.Value.ToString() == NameOfKeywordText);
        }

        public static bool IsStringEmpty(this ExpressionSyntax expression, SemanticModel semanticModel)
        {
            var memberAccessExpression = expression as MemberAccessExpressionSyntax;
            if (memberAccessExpression == null)
            {
                return false;
            }

            var name = semanticModel.GetSymbolInfo(memberAccessExpression.Name);

            return name.Symbol != null &&
                   name.Symbol.IsInType(KnownType.System_String) &&
                   name.Symbol.Name == nameof(string.Empty);
        }

        public static bool IsAnyKind(this SyntaxNode syntaxNode, ICollection<SyntaxKind> collection)
        {
            return syntaxNode != null && collection.Contains((SyntaxKind)syntaxNode.RawKind);
        }

        public static bool IsAnyKind(this SyntaxToken syntaxToken, ICollection<SyntaxKind> collection)
        {
            return collection.Contains((SyntaxKind)syntaxToken.RawKind);
        }

        public static bool ContainsMethodInvocation(BaseMethodDeclarationSyntax methodDeclarationBase,
            SemanticModel semanticModel,
            Func<InvocationExpressionSyntax, bool> syntaxPredicate, Func<IMethodSymbol, bool> symbolPredicate)
        {
            IEnumerable<SyntaxNode> childNodes = methodDeclarationBase?.Body?.DescendantNodes();
            if (childNodes == null)
            {
                childNodes = (methodDeclarationBase as MethodDeclarationSyntax)?.ExpressionBody?.DescendantNodes();
            }

            return childNodes != null &&
                ContainsMethodInvocation(childNodes, semanticModel, syntaxPredicate, symbolPredicate);
        }

        private static bool ContainsMethodInvocation(IEnumerable<SyntaxNode> syntaxNodes, SemanticModel semanticModel,
            Func<InvocationExpressionSyntax, bool> syntaxPredicate, Func<IMethodSymbol, bool> symbolPredicate)
        {
            // See issue: https://github.com/SonarSource/sonar-csharp/issues/416
            // Where clause excludes nodes that are not defined on the same SyntaxTree as the
            // SemanticModel (because of partial definition).

            // Another approach would be to get the SemanticModel linked to the node we explore.
            // More details: https://github.com/dotnet/roslyn/issues/18730
            return syntaxNodes
                .OfType<InvocationExpressionSyntax>()
                .Where(syntaxPredicate)
                .Where(invocation => invocation.SyntaxTree.Equals(semanticModel.SyntaxTree))
                .Select(e => semanticModel.GetSymbolInfo(e.Expression).Symbol)
                .OfType<IMethodSymbol>()
                .Any(symbolPredicate);
        }

        public static Location FindIdentifierLocation(this BaseMethodDeclarationSyntax methodDeclaration)
        {
            var identifierSyntax = (methodDeclaration as MethodDeclarationSyntax)?.Identifier ??
                                   (methodDeclaration as ConstructorDeclarationSyntax)?.Identifier ??
                                   (methodDeclaration as DestructorDeclarationSyntax)?.Identifier;

            return identifierSyntax?.GetLocation();
        }

        public static int GetLineNumber(this SyntaxToken token)
        {
            return token.GetLocation().GetLineSpan().StartLinePosition.Line;
        }

        public static bool IsCatchingAllExceptions(this CatchClauseSyntax catchClause)
        {
            if (catchClause.Declaration == null)
            {
                return true;
            }

            var exceptionTypeName = catchClause.Declaration.Type.GetText().ToString();

            return catchClause.Filter == null &&
                (exceptionTypeName == "Exception" || exceptionTypeName == "System.Exception");
        }
    }
}
