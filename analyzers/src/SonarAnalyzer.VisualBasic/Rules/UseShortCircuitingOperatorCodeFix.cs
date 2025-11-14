/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.VisualBasic.Rules
{
    [ExportCodeFixProvider(LanguageNames.VisualBasic)]
    public sealed class UseShortCircuitingOperatorCodeFix : UseShortCircuitingOperatorCodeFixBase<SyntaxKind, BinaryExpressionSyntax>
    {
        internal override bool IsCandidateExpression(BinaryExpressionSyntax expression)
        {
            return UseShortCircuitingOperator.ShortCircuitingAlternative.ContainsKey(expression.Kind());
        }

        protected override BinaryExpressionSyntax GetShortCircuitingExpressionNode(BinaryExpressionSyntax expression)
        {
            return expression.IsKind(SyntaxKind.AndExpression)
                ? SyntaxFactory.AndAlsoExpression(expression.Left, expression.Right)
                : SyntaxFactory.OrElseExpression(expression.Left, expression.Right);
        }
    }
}
