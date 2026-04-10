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

using System.Text.RegularExpressions;

namespace SonarAnalyzer.Core.RegularExpressions.Test;

[TestClass]
public class RegexContextTest
{
    [TestMethod]
    [DataRow("[A", RegexOptions.None)]
#if NET
    [DataRow(@"^([0-9]{2})(?<!00)$", RegexOptions.NonBacktracking)]
#endif
    public void InvalidInput_SetParseError(string pattern, RegexOptions options) =>
        new RegexContext(null, pattern, null, options).ParseError.Should().NotBeNull();
}
