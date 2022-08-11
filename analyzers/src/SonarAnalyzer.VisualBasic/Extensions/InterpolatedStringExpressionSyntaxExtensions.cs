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

using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Extensions
{
    public static class InterpolatedStringExpressionSyntaxExtensions
    {
        public static string GetContentsText(this InterpolatedStringExpressionSyntax interpolatedStringExpression) =>
            interpolatedStringExpression.Contents.JoinStr(null, content => content.ToString());

        public static bool TryGetGetInterpolatedTextValue(this InterpolatedStringExpressionSyntax interpolatedStringExpression, SemanticModel semanticModel, out string interpolatedValue)
        {
            var resolvedContent = new StringBuilder();
            foreach (var interpolatedStringContent in interpolatedStringExpression.Contents)
            {
                if (interpolatedStringContent is InterpolationSyntax interpolation)
                {
                    if (interpolation.Expression is InterpolatedStringExpressionSyntax nestedInterpolatedString
                        && TryGetGetInterpolatedTextValue(nestedInterpolatedString, semanticModel, out var innerInterpolatedValue))
                    {
                        resolvedContent.Append(innerInterpolatedValue);
                    }
                    else if (interpolation.Expression.FindConstantValue(semanticModel) is string constantValue)
                    {
                        resolvedContent.Append(constantValue);
                    }
                    else
                    {
                        interpolatedValue = null;
                        return false;
                    }
                }
                else if (interpolatedStringContent is InterpolatedStringTextSyntax interpolatedText)
                {
                    resolvedContent.Append(interpolatedText.TextToken.Text);
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
