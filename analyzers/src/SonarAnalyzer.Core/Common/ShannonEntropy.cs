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

namespace SonarAnalyzer.Core.Common;

public static class ShannonEntropy
{
    // See https://en.wikipedia.org/wiki/Entropy_(information_theory) for more details.
    public static double Calculate(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return 0;
        }

        var length = input.Length;
        return input
            .GroupBy(x => x)
            .ToDictionary(x => x.Key, x => x.Count())
            .Values
            .Select(x => (double)x / length)
            .Select(probability => -probability * Math.Log(probability, 2))
            .Sum();
    }
}
