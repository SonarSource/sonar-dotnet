/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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

using Microsoft.CodeAnalysis;

namespace SonarAnalyzer.Helpers
{
    public abstract class StringValueHelper<TSyntaxKind,
                                            TInterpolatedStringExpressionSyntax,
                                            TLiteralExpressionSyntax>
        where TSyntaxKind : struct
        where TInterpolatedStringExpressionSyntax : SyntaxNode
        where TLiteralExpressionSyntax : SyntaxNode
    {
        protected abstract ILanguageFacade<TSyntaxKind> Language { get; }

        protected abstract SyntaxToken Token(TLiteralExpressionSyntax literalExpression);

        public string GetStringValue(SyntaxNode node, SemanticModel semanticModel)
        {
            if (node != null)
            {
                if (Language.Syntax.IsKind(node, Language.SyntaxKind.StringLiteralExpression)
                    && node is TLiteralExpressionSyntax literal)
                {
                    return Token(literal).ValueText;
                }
                else if (Language.Syntax.IsKind(node, Language.SyntaxKind.InterpolatedStringExpression)
                        && (TInterpolatedStringExpressionSyntax)node is var interpolatedStringExpression
                        && Language.Syntax.TryGetGetInterpolatedTextValue(interpolatedStringExpression, semanticModel, out var interpolatedValue))
                {
                    return interpolatedValue;
                }
            }
            return null;
        }
    }
}
