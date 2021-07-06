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

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Extensions;
using SonarAnalyzer.Helpers;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class RedundantNullCheck : RedundantNullCheckBase<BinaryExpressionSyntax>
    {
        private const string MessageFormat = "Remove this unnecessary null check; 'is' returns false for nulls.";

        private static readonly DiagnosticDescriptor Rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                CheckAndExpression,
                SyntaxKind.LogicalAndExpression);

            context.RegisterSyntaxNodeActionInNonGenerated(
                CheckOrExpression,
                SyntaxKind.LogicalOrExpression);
        }

        protected override SyntaxNode GetLeftNode(BinaryExpressionSyntax binaryExpression) => binaryExpression.Left.RemoveParentheses();

        protected override SyntaxNode GetRightNode(BinaryExpressionSyntax binaryExpression) => binaryExpression.Right.RemoveParentheses();

        private SyntaxNode GetNullCheckVariableForKind(SyntaxNode node, SyntaxKind kind) {
            if (node.RemoveParentheses() is BinaryExpressionSyntax binaryExpression && binaryExpression.IsKind(kind))
            {
                var leftNode = GetLeftNode(binaryExpression);
                var rightNode = GetRightNode(binaryExpression);

                if (leftNode.IsNullLiteral())
                {
                    return rightNode;
                }
                if (rightNode.IsNullLiteral())
                {
                    return leftNode;
                }
            }
            return null;
        }

        protected override SyntaxNode GetNullCheckVariable(SyntaxNode node) => GetNullCheckVariableForKind(node, SyntaxKind.EqualsExpression);

        protected override SyntaxNode GetNonNullCheckVariable(SyntaxNode node) => GetNullCheckVariableForKind(node, SyntaxKind.NotEqualsExpression);

        protected override SyntaxNode GetIsOperatorCheckVariable(SyntaxNode node)
        {
            var innerExpression = node.RemoveParentheses();
            if (innerExpression is BinaryExpressionSyntax binaryExpression && binaryExpression.IsKind(SyntaxKind.IsExpression))
            {
                return GetLeftNode(binaryExpression);
            }
            else if (innerExpression.IsKind(SyntaxKindEx.IsPatternExpression))
            {
                var isPatternExpression = (IsPatternExpressionSyntaxWrapper)innerExpression;
                return isPatternExpression.Expression.RemoveParentheses();
            }
            return null;
        }

        protected override SyntaxNode GetInvertedIsOperatorCheckVariable(SyntaxNode node) =>
            node.RemoveParentheses() is PrefixUnaryExpressionSyntax prefixUnary && prefixUnary.IsKind(SyntaxKind.LogicalNotExpression)
                ? GetIsOperatorCheckVariable(prefixUnary.Operand)
                : null;

        protected override bool AreEquivalent(SyntaxNode node1, SyntaxNode node2) => CSharpEquivalenceChecker.AreEquivalent(node1, node2);
    }
}
