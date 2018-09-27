/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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

        public static bool IsAnyKind(this SyntaxToken syntaxToken, ISet<SyntaxKind> collection) =>
            collection.Contains((SyntaxKind)syntaxToken.RawKind);

        public static bool IsAnyKind(this SyntaxNode syntaxNode, ISet<SyntaxKind> collection) =>
            syntaxNode != null && collection.Contains((SyntaxKind)syntaxNode.RawKind);

        public static bool AnyOfKind(this IEnumerable<SyntaxNode> nodes, SyntaxKind kind) =>
            nodes.Any(n => n.RawKind == (int)kind);

        public static bool AnyOfKind(this IEnumerable<SyntaxToken> tokens, SyntaxKind kind) =>
            tokens.Any(n => n.RawKind == (int)kind);
    }
}
