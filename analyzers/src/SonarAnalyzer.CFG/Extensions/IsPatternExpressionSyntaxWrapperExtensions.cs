/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
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
    extension(IsPatternExpressionSyntaxWrapper isPatternWrapper)
    {
        public bool IsNull => isPatternWrapper.Pattern.IsNull;

        public bool IsNot =>
            isPatternWrapper.Pattern.WithoutEnclosingParentheses is var syntaxNode
            && UnaryPatternSyntaxWrapper.IsInstance(syntaxNode)
            && ((UnaryPatternSyntaxWrapper)syntaxNode) is var unaryPatternSyntaxWrapper
            && unaryPatternSyntaxWrapper.IsNot;

        public bool IsNotNull =>
            isPatternWrapper.Pattern.WithoutEnclosingParentheses is var syntaxNode
            && UnaryPatternSyntaxWrapper.IsInstance(syntaxNode)
            && ((UnaryPatternSyntaxWrapper)syntaxNode) is var unaryPatternSyntaxWrapper
            && unaryPatternSyntaxWrapper.IsNotNull;
    }
}
