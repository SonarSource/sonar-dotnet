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

using System.Text;

namespace SonarAnalyzer.Core.Syntax.Utilities;

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

    public string InterpolatedTextValue(TInterpolatedStringExpressionSyntax interpolatedStringExpression, SemanticModel model)
    {
        var resolvedContent = new StringBuilder();
        foreach (var interpolatedStringContent in Contents(interpolatedStringExpression))
        {
            if (interpolatedStringContent is TInterpolationSyntax interpolation)
            {
                if (Language.Syntax.NodeExpression(interpolation) is TInterpolatedStringExpressionSyntax nestedInterpolatedString
                    && InterpolatedTextValue(nestedInterpolatedString, model) is { } innerInterpolatedValue)
                {
                    resolvedContent.Append(innerInterpolatedValue);
                }
                else if (Language.FindConstantValue(model, Language.Syntax.NodeExpression(interpolation)) is string constantValue)
                {
                    resolvedContent.Append(constantValue);
                }
                else
                {
                    return null;
                }
            }
            else if (interpolatedStringContent is TInterpolatedStringTextSyntax interpolatedText)
            {
                resolvedContent.Append(TextToken(interpolatedText).Text);
            }
            else
            {
                return null;
            }
        }
        return resolvedContent.ToString();
    }
}
