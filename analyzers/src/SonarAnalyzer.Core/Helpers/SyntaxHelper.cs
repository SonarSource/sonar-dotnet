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

namespace SonarAnalyzer.Helpers;

internal static class SyntaxHelper
{
    public static IEnumerable<int> GetLineNumbers(this SyntaxToken token, bool isZeroBasedCount = true)
        => token.GetLocation().GetLineSpan().GetLineNumbers(isZeroBasedCount);

    public static IEnumerable<int> GetLineNumbers(this SyntaxTrivia trivia, bool isZeroBasedCount = true)
        => trivia.GetLocation().GetLineSpan().GetLineNumbers(isZeroBasedCount);

    public static IEnumerable<int> GetLineNumbers(this FileLinePositionSpan lineSpan, bool isZeroBasedCount = true)
    {
        var offset = isZeroBasedCount ? 0 : 1;
        var start = lineSpan.StartLinePosition.Line + offset;
        var end = lineSpan.EndLinePosition.Line + offset;
        return Enumerable.Range(start, end - start + 1);
    }
}
