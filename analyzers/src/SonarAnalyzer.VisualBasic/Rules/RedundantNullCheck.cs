/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    public sealed class RedundantNullCheck : RedundantNullCheckBase<BinaryExpressionSyntax>
    {
        private const string MessageFormat = "Remove this unnecessary null check; 'TypeOf ... Is' returns false for nulls.";

        private static readonly DiagnosticDescriptor rule =
           DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(
                CheckAndExpression,
                SyntaxKind.AndExpression,
                SyntaxKind.AndAlsoExpression);

            context.RegisterNodeAction(
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
