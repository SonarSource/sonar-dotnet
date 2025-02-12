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

namespace SonarAnalyzer.Core.Syntax.Extensions;

public static class FileLinePositionSpanExtensions
{
    public static IEnumerable<int> LineNumbers(this FileLinePositionSpan lineSpan, bool isZeroBasedCount = true)
    {
        var offset = isZeroBasedCount ? 0 : 1;
        var start = lineSpan.StartLinePosition.Line + offset;
        var end = lineSpan.EndLinePosition.Line + offset;
        return Enumerable.Range(start, end - start + 1);
    }
}
