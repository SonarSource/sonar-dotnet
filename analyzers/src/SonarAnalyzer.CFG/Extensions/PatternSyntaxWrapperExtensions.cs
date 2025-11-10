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

namespace SonarAnalyzer.CFG.Extensions;

public static class PatternSyntaxWrapperExtensions
{
    public static bool IsNull(this PatternSyntaxWrapper patternSyntaxWrapper) =>
        patternSyntaxWrapper.RemoveParentheses() is var syntaxNode
        && ConstantPatternSyntaxWrapper.IsInstance(syntaxNode)
        && (ConstantPatternSyntaxWrapper)syntaxNode is var constantPattern
        && constantPattern.Expression.Kind() == SyntaxKind.NullLiteralExpression;

    public static bool IsNot(this PatternSyntaxWrapper patternSyntaxWrapper) =>
        patternSyntaxWrapper.RemoveParentheses().Kind() == SyntaxKindEx.NotPattern;

    public static SyntaxNode RemoveParentheses(this PatternSyntaxWrapper patternSyntaxWrapper) =>
        patternSyntaxWrapper.SyntaxNode.RemoveParentheses();
}
