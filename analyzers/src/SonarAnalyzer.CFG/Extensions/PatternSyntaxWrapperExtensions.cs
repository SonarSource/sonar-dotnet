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

using Microsoft.CodeAnalysis.CSharp;

namespace SonarAnalyzer.CFG.Extensions;

public static class PatternSyntaxWrapperExtensions
{
    extension(PatternSyntaxWrapper patternSyntaxWrapper)
    {
        public bool IsNull =>
            patternSyntaxWrapper.WithoutEnclosingParentheses is var syntaxNode
            && ConstantPatternSyntaxWrapper.IsInstance(syntaxNode)
            && (ConstantPatternSyntaxWrapper)syntaxNode is var constantPattern
            && constantPattern.Expression.Kind() == SyntaxKind.NullLiteralExpression;

        public bool IsNot => patternSyntaxWrapper.WithoutEnclosingParentheses.Kind() == SyntaxKindEx.NotPattern;

        public SyntaxNode WithoutEnclosingParentheses => patternSyntaxWrapper.Node.WithoutEnclosingParentheses;
    }
}
