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

namespace SonarAnalyzer.CSharp.Rules
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    public class UseShortCircuitingOperatorCodeFix : UseShortCircuitingOperatorCodeFixBase<SyntaxKind, BinaryExpressionSyntax>
    {
        internal override bool IsCandidateExpression(BinaryExpressionSyntax expression)
        {
            return UseShortCircuitingOperator.ShortCircuitingAlternative.ContainsKey(expression.Kind());
        }

        protected override BinaryExpressionSyntax GetShortCircuitingExpressionNode(BinaryExpressionSyntax expression)
        {
            var alternativeKind = expression.IsKind(SyntaxKind.BitwiseAndExpression)
                ? SyntaxKind.LogicalAndExpression
                : SyntaxKind.LogicalOrExpression;

            return SyntaxFactory.BinaryExpression(alternativeKind, expression.Left, expression.Right);
        }
    }
}
