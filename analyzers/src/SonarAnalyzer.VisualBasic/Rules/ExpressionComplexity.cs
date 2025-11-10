/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

namespace SonarAnalyzer.VisualBasic.Rules
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    public sealed class ExpressionComplexity : ExpressionComplexityBase<SyntaxKind>
    {
        protected override ILanguageFacade Language { get; } = VisualBasicFacade.Instance;

        protected override HashSet<SyntaxKind> TransparentKinds { get; } =
            [
                SyntaxKind.ParenthesizedExpression,
                SyntaxKind.NotExpression,
            ];

        protected override HashSet<SyntaxKind> ComplexityIncreasingKinds { get; } =
            [
                SyntaxKind.AndExpression,
                SyntaxKind.AndAlsoExpression,
                SyntaxKind.OrExpression,
                SyntaxKind.OrElseExpression,
                SyntaxKind.ExclusiveOrExpression
            ];

        protected override SyntaxNode[] ExpressionChildren(SyntaxNode node) =>
            node switch
            {
                BinaryExpressionSyntax
                {
                    RawKind: (int)SyntaxKind.AndExpression
                    or (int)SyntaxKind.AndAlsoExpression
                    or (int)SyntaxKind.OrExpression
                    or (int)SyntaxKind.OrElseExpression
                    or (int)SyntaxKind.ExclusiveOrExpression
                } binary => new[] { binary.Left, binary.Right },
                ParenthesizedExpressionSyntax parenthesized => new[] { parenthesized.Expression },
                UnaryExpressionSyntax { RawKind: (int)SyntaxKind.NotExpression } unary => new[] { unary.Operand },
                _ => Array.Empty<SyntaxNode>(),
            };
    }
}
