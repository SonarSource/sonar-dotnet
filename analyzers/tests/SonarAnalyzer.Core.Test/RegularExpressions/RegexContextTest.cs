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
using SonarAnalyzer.Core.RegularExpressions;

namespace SonarAnalyzer.Core.Test.RegularExpressions;

[TestClass]
public class RegexContextTest
{
    [DataTestMethod]
    [DataRow("[A", RegexOptions.None)]
#if NET
    [DataRow(@"^([0-9]{2})(?<!00)$", RegexOptions.NonBacktracking)]
#endif
    public void InvalidInput_SetParseError(string pattern, RegexOptions options) =>
        new RegexContext(null, pattern, null, options).ParseError.Should().NotBeNull();
}
