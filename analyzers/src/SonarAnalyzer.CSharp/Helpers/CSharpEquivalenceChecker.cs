/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.Extensions;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Helpers
{
    internal static class CSharpEquivalenceChecker
    {
        public static bool AreEquivalent(SyntaxNode node1, SyntaxNode node2) =>
            Common.EquivalenceChecker.AreEquivalent(node1, node2, NodeComparator);

        public static bool AreEquivalent(SyntaxList<SyntaxNode> nodeList1, SyntaxList<SyntaxNode> nodeList2) =>
            Common.EquivalenceChecker.AreEquivalent(nodeList1, nodeList2, NodeComparator);

        private static bool NodeComparator(SyntaxNode node1, SyntaxNode node2) =>
            NullCheckState(node1, true) is { } nullCheck1
            && NullCheckState(node2, true) is { } nullCheck2
            && nullCheck1.IsPositive == nullCheck2.IsPositive
                ? SyntaxFactory.AreEquivalent(nullCheck1.Expression, nullCheck2.Expression)
                : SyntaxFactory.AreEquivalent(node1, node2);

        /// <param name="isPositive">Flag indicating that current chain of null checks is positive '== null'. It's flipped for each `!` operator. '!(x != null)' is equal to 'x == null'.</param>
        private static NullCheck NullCheckState(SyntaxNode node, bool isPositive)
        {
            if (node is PrefixUnaryExpressionSyntax unary && unary.IsKind(SyntaxKind.LogicalNotExpression))
            {
                return NullCheckState(unary.Operand.RemoveParentheses(), !isPositive);
            }
            else if (node is BinaryExpressionSyntax binary && binary.IsAnyKind(SyntaxKind.EqualsExpression, SyntaxKind.NotEqualsExpression))
            {
                if (binary.IsKind(SyntaxKind.NotEqualsExpression))
                {
                    isPositive = !isPositive;
                }
                return NullCheckExpression(binary) is { } expression ? new NullCheck(expression, isPositive) : null;
            }
            else if (node.IsKind(SyntaxKindEx.IsPatternExpression))
            {
                var isPattern = (IsPatternExpressionSyntaxWrapper)node;
                return NullCheckPattern(isPattern.Expression, isPattern.Pattern.SyntaxNode, isPositive);
            }
            else
            {
                return null;
            }
        }

        private static SyntaxNode NullCheckExpression(BinaryExpressionSyntax binary)
        {
            if (binary.Left.IsKind(SyntaxKind.NullLiteralExpression))
            {
                return binary.Right;
            }
            else if (binary.Right.IsKind(SyntaxKind.NullLiteralExpression))
            {
                return binary.Left;
            }
            else
            {
                return null;
            }
        }

        /// <param name="isPositive">Flag indicating that current chain of null checks is positive 'is null'. It's flipped for each `not` operator. 'is not not null' is equal to 'is null'.</param>
        private static NullCheck NullCheckPattern(SyntaxNode expression, SyntaxNode pattern, bool isPositive)
        {
            if (pattern.IsKind(SyntaxKindEx.ConstantPattern) && ((ConstantPatternSyntaxWrapper)pattern).Expression.IsKind(SyntaxKind.NullLiteralExpression))
            {
                return new NullCheck(expression, isPositive);
            }
            else if (pattern.IsKind(SyntaxKindEx.NotPattern))
            {
                return NullCheckPattern(expression, ((UnaryPatternSyntaxWrapper)pattern).Pattern.SyntaxNode, !isPositive);
            }
            else
            {
                return null;
            }
        }

        private class NullCheck
        {
            public readonly SyntaxNode Expression;
            public readonly bool IsPositive;

            public NullCheck(SyntaxNode expression, bool isPositive)
            {
                Expression = expression;
                IsPositive = isPositive;
            }
        }
    }

    internal class CSharpSyntaxNodeEqualityComparer<T> : IEqualityComparer<T>, IEqualityComparer<SyntaxList<T>>
        where T : SyntaxNode
    {
        public bool Equals(T x, T y) =>
            CSharpEquivalenceChecker.AreEquivalent(x, y);

        public bool Equals(SyntaxList<T> x, SyntaxList<T> y) =>
            CSharpEquivalenceChecker.AreEquivalent(x, y);

        public int GetHashCode(T obj) =>
            obj.GetType().FullName.GetHashCode();

        public int GetHashCode(SyntaxList<T> obj) =>
            (obj.Count + obj.Select(x => x.GetType().FullName).Distinct().JoinStr(", ")).GetHashCode();
    }

}
