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

namespace SonarAnalyzer.CFG.Extensions;

public static class IsPatternExpressionSyntaxWrapperExtensions
{
    public static bool IsNull(this IsPatternExpressionSyntaxWrapper isPatternWrapper) =>
        isPatternWrapper.Pattern.IsNull();

    public static bool IsNot(this IsPatternExpressionSyntaxWrapper isPatternWrapper) =>
        isPatternWrapper.Pattern.RemoveParentheses() is var syntaxNode
        && UnaryPatternSyntaxWrapper.IsInstance(syntaxNode)
        && ((UnaryPatternSyntaxWrapper)syntaxNode) is var unaryPatternSyntaxWrapper
        && unaryPatternSyntaxWrapper.IsNot();

    public static bool IsNotNull(this IsPatternExpressionSyntaxWrapper isPatternWrapper) =>
        isPatternWrapper.Pattern.RemoveParentheses() is var syntaxNode
        && UnaryPatternSyntaxWrapper.IsInstance(syntaxNode)
        && ((UnaryPatternSyntaxWrapper)syntaxNode) is var unaryPatternSyntaxWrapper
        && unaryPatternSyntaxWrapper.IsNotNull();
}
