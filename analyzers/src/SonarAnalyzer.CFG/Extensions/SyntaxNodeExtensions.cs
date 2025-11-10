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

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SonarAnalyzer.CFG.Extensions;

internal static class SyntaxNodeExtensions
{
    private static readonly ISet<SyntaxKind> ParenthesizedExpressionKinds = new HashSet<SyntaxKind> { SyntaxKind.ParenthesizedExpression, SyntaxKindEx.ParenthesizedPattern };

    public static SyntaxNode RemoveParentheses(this SyntaxNode expression)
    {
        var currentExpression = expression;
        while (currentExpression is not null && ParenthesizedExpressionKinds.Contains(currentExpression.Kind()))
        {
            if (currentExpression.IsKind(SyntaxKind.ParenthesizedExpression))
            {
                currentExpression = ((ParenthesizedExpressionSyntax)currentExpression).Expression;
            }
            else
            {
                currentExpression = ((ParenthesizedPatternSyntaxWrapper)currentExpression).Pattern;
            }
        }
        return currentExpression;
    }
}
