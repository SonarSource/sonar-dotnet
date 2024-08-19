/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    public sealed class ExpressionComplexity : ExpressionComplexityBase<SyntaxKind>
    {
        protected override ILanguageFacade Language { get; } = VisualBasicFacade.Instance;

        protected override SyntaxKind[] TransparentKinds { get; } =
            {
                SyntaxKind.ParenthesizedExpression,
                SyntaxKind.NotExpression,
            };

        protected override SyntaxKind[] ComplexityIncreasingKinds { get; } =
            {
                SyntaxKind.AndExpression,
                SyntaxKind.AndAlsoExpression,
                SyntaxKind.OrExpression,
                SyntaxKind.OrElseExpression,
                SyntaxKind.ExclusiveOrExpression
            };

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
