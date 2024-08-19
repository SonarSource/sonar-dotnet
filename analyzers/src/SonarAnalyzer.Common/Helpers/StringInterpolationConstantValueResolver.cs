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

using System.Text;

namespace SonarAnalyzer.Helpers
{
    public abstract class StringInterpolationConstantValueResolver<TSyntaxKind,
                                                                   TInterpolatedStringExpressionSyntax,
                                                                   TInterpolatedStringContentSyntax,
                                                                   TInterpolationSyntax,
                                                                   TInterpolatedStringTextSyntax>
        where TSyntaxKind : struct
        where TInterpolatedStringExpressionSyntax : SyntaxNode
        where TInterpolatedStringContentSyntax : SyntaxNode
        where TInterpolationSyntax : SyntaxNode
        where TInterpolatedStringTextSyntax : SyntaxNode
    {
        protected abstract ILanguageFacade<TSyntaxKind> Language { get; }

        protected abstract IEnumerable<TInterpolatedStringContentSyntax> Contents(TInterpolatedStringExpressionSyntax interpolatedStringExpression);
        protected abstract SyntaxToken TextToken(TInterpolatedStringTextSyntax interpolatedStringText);

        public bool TryGetInterpolatedTextValue(TInterpolatedStringExpressionSyntax interpolatedStringExpression, SemanticModel semanticModel, out string interpolatedValue)
        {
            var resolvedContent = new StringBuilder();
            foreach (var interpolatedStringContent in Contents(interpolatedStringExpression))
            {
                if (interpolatedStringContent is TInterpolationSyntax interpolation)
                {
                    if (Language.Syntax.NodeExpression(interpolation) is TInterpolatedStringExpressionSyntax nestedInterpolatedString
                        && TryGetInterpolatedTextValue(nestedInterpolatedString, semanticModel, out var innerInterpolatedValue))
                    {
                        resolvedContent.Append(innerInterpolatedValue);
                    }
                    else if (Language.FindConstantValue(semanticModel, Language.Syntax.NodeExpression(interpolation)) is string constantValue)
                    {
                        resolvedContent.Append(constantValue);
                    }
                    else
                    {
                        interpolatedValue = null;
                        return false;
                    }
                }
                else if (interpolatedStringContent is TInterpolatedStringTextSyntax interpolatedText)
                {
                    resolvedContent.Append(TextToken(interpolatedText).Text);
                }
                else
                {
                    interpolatedValue = null;
                    return false;
                }
            }
            interpolatedValue = resolvedContent.ToString();
            return true;
        }
    }
}
