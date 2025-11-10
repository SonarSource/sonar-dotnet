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

namespace SonarAnalyzer.CSharp.Core.Syntax.Extensions;

public static class BlockSyntaxExtensions
{
    public static bool IsEmpty(this BlockSyntax block, bool treatCommentsAsContent = true, bool treatConditionalCompilationAsContent = true)
    {
        _ = block ?? throw new ArgumentNullException(nameof(block));
        return !IsNotEmpty();

        bool IsNotEmpty() =>
            block.Statements.Any()
            || ((treatCommentsAsContent || treatConditionalCompilationAsContent)
                && (TriviaContainsCommentOrConditionalCompilation(block.OpenBraceToken.TrailingTrivia)
                    || TriviaContainsCommentOrConditionalCompilation(block.CloseBraceToken.LeadingTrivia)));

        bool TriviaContainsCommentOrConditionalCompilation(SyntaxTriviaList triviaList) =>
            triviaList.Any(x =>
                (treatCommentsAsContent && x.Kind() is SyntaxKind.SingleLineCommentTrivia or SyntaxKind.MultiLineCommentTrivia)
                 || (treatConditionalCompilationAsContent && x.IsKind(SyntaxKind.DisabledTextTrivia)));
    }
}
