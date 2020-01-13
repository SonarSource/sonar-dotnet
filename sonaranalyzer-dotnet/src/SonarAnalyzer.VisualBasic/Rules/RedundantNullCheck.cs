/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2020 SonarSource SA
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
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Helpers.VisualBasic;

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    [Rule(DiagnosticId)]
    public sealed class RedundantNullCheck : RedundantNullCheckBase<BinaryExpressionSyntax>
    {
        private const string MessageFormat = "Remove this unnecessary null check; 'TypeOf ... Is' returns false for nulls.";

        private static readonly DiagnosticDescriptor rule =
           DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                CheckAndExpression,
                SyntaxKind.AndExpression,
                SyntaxKind.AndAlsoExpression);

            context.RegisterSyntaxNodeActionInNonGenerated(
                CheckOrExpression,
                SyntaxKind.OrExpression,
                SyntaxKind.OrElseExpression);
        }

        protected override SyntaxNode GetLeftNode(BinaryExpressionSyntax binaryExpression) => binaryExpression.Left.RemoveParentheses();

        protected override SyntaxNode GetRightNode(BinaryExpressionSyntax binaryExpression) => binaryExpression.Right.RemoveParentheses();

        protected override SyntaxNode GetNullCheckVariable(SyntaxNode node)
        {
            if (RemoveParentheses(node) is BinaryExpressionSyntax binaryExpression
               && binaryExpression.IsKind(SyntaxKind.IsExpression)
               && GetRightNode(binaryExpression).IsKind(SyntaxKind.NothingLiteralExpression))
            {
                return GetLeftNode(binaryExpression);
            }
            return null;
        }

        protected override SyntaxNode GetNonNullCheckVariable(SyntaxNode node)
        {
            var innerExpression = RemoveParentheses(node);
            if (innerExpression is BinaryExpressionSyntax binaryExpression
               && binaryExpression.IsKind(SyntaxKind.IsNotExpression)
               && GetRightNode(binaryExpression).IsKind(SyntaxKind.NothingLiteralExpression))
            {
                return GetLeftNode(binaryExpression);
            }
            else if (innerExpression is UnaryExpressionSyntax prefixUnary && prefixUnary.IsKind(SyntaxKind.NotExpression))
            {
                return GetNullCheckVariable(prefixUnary.Operand);
            }
            return null;
        }

        protected override SyntaxNode GetIsOperatorCheckVariable(SyntaxNode node)
        {
            if (RemoveParentheses(node) is TypeOfExpressionSyntax typeOfExpression && typeOfExpression.OperatorToken.IsKind(SyntaxKind.IsKeyword))
            {
                return typeOfExpression.Expression.RemoveParentheses();
            }
            return null;
        }

        protected override SyntaxNode GetInvertedIsOperatorCheckVariable(SyntaxNode node)
        {
            var innerExpression = RemoveParentheses(node);
            if (innerExpression is UnaryExpressionSyntax prefixUnary && prefixUnary.IsKind(SyntaxKind.NotExpression))
            {
                return GetIsOperatorCheckVariable(prefixUnary.Operand);
            }
            else if (innerExpression is TypeOfExpressionSyntax typeOfExpression && typeOfExpression.OperatorToken.IsKind(SyntaxKind.IsNotKeyword))
            {
                return typeOfExpression.Expression.RemoveParentheses();
            }
            return null;
        }

        protected override bool AreEquivalent(SyntaxNode node1, SyntaxNode node2) => VisualBasicEquivalenceChecker.AreEquivalent(node1, node2);

        private SyntaxNode RemoveParentheses(SyntaxNode syntaxNode) => syntaxNode.RemoveParentheses();
    }
}
