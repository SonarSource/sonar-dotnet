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

using System.Text.RegularExpressions;

namespace SonarAnalyzer.Core.RegularExpressions;

public static class NamingPatterns
{
    public const string PascalCasingInternalPattern = "([A-Z]{1,3}[a-z0-9]+)*" + MaxTwoLongIdPattern;
    public const string PascalCasingPattern = "^" + PascalCasingInternalPattern + "$";
    public const string CamelCasingPattern = "^" + CamelCasingInternalPattern + "$";
    public const string CamelCasingPatternWithOptionalPrefixes = "^(s_|_)?" + CamelCasingInternalPattern + "$";
    private const string CamelCasingInternalPattern = "[a-z][a-z0-9]*" + PascalCasingInternalPattern;
    private const string MaxTwoLongIdPattern = "([A-Z]{2})?";

    internal static bool IsRegexMatch(string name, string pattern, bool timeoutFallback = false) =>
        SafeRegex.IsMatch(name, pattern, RegexOptions.CultureInvariant | RegexOptions.Compiled, timeoutFallback);
}
