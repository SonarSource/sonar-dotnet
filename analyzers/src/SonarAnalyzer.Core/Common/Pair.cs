/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.Core.Common;

public readonly record struct Pair<TLeft, TRight>(TLeft Left, TRight Right);

public static class Pair
{
    public static Pair<TLeft, TRight> From<TLeft, TRight>(TLeft left, TRight right) =>
        new(left, right);

    public static void Swap<T>(ref T left, ref T right)
    {
        var tmp = left;
        left = right;
        right = tmp;
    }
}
