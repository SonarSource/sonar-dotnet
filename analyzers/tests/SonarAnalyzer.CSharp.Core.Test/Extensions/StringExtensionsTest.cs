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

namespace SonarAnalyzer.CSharp.Core.Test.Extensions;

[TestClass]
public class StringExtensionsTest
{
    [TestMethod]
    [DataRow("token", "token", "token")]    // plain identifier
    [DataRow("_ct", "_ct", "_ct")]          // plain identifier with underscore
    [DataRow("this", "@this", "this")]      // reserved keyword
    [DataRow("class", "@class", "class")]   // reserved keyword
    [DataRow("return", "@return", "return")] // reserved keyword
    [DataRow("var", "@var", "var")]         // contextual keyword
    [DataRow("async", "@async", "async")]   // contextual keyword
    [DataRow("await", "@await", "await")]   // contextual keyword
    public void EscapedIdentifierName(string name, string expectedText, string expectedValueText)
    {
        var result = name.EscapedIdentifierName;
        result.Identifier.Text.Should().Be(expectedText);
        result.Identifier.ValueText.Should().Be(expectedValueText);
    }
}
