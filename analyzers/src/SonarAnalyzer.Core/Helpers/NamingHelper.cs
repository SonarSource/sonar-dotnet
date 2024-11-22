/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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

namespace SonarAnalyzer.Helpers;

public static class NamingHelper
{
    internal const string MaxTwoLongIdPattern = "([A-Z]{2})?";
    internal const string PascalCasingInternalPattern = "([A-Z]{1,3}[a-z0-9]+)*" + MaxTwoLongIdPattern;
    internal const string CamelCasingInternalPattern = "[a-z][a-z0-9]*" + PascalCasingInternalPattern;
    internal const string PascalCasingPattern = "^" + PascalCasingInternalPattern + "$";
    internal const string CamelCasingPattern = "^" + CamelCasingInternalPattern + "$";
    internal const string CamelCasingPatternWithOptionalPrefixes = "^(s_|_)?" + CamelCasingInternalPattern + "$";

    internal static bool IsRegexMatch(string name, string pattern) =>
        SafeRegex.IsMatch(name, pattern, RegexOptions.CultureInvariant | RegexOptions.Compiled);
}
