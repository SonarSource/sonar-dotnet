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

namespace SonarAnalyzer.Test.Helpers;

[TestClass]
public class VbcHelperTest
{
    [DataTestMethod]
    [DataRow(null, false)]
    [DataRow("error", false)]
    [DataRow("error:", false)]
    [DataRow(" error:", true)]
    [DataRow("   error:", true)]
    [DataRow("   error", false)]
    [DataRow(" error :", true)]
    [DataRow(" ErRoR :", true)]
    [DataRow(" error   :", true)]
    [DataRow(" error   ", false)]
    [DataRow("error foo", false)]
    [DataRow("error foo:", false)]
    [DataRow(" error foo:", true)]
    [DataRow(" error foo :", true)]
    [DataRow(" error   foo:", true)]
    [DataRow(" error foo   :", true)]
    [DataRow(" errorfoo:", false)]
    [DataRow("   error foo:", true)]
    [DataRow("   eRrOr fOo:", true)]
    [DataRow("   error in foo:", false)]
    [DataRow("   error in foo :", false)]
    public void IsTextMatchingVbcErrorPattern_ReturnsExpected(string text, bool expectedResult) =>
        VbcHelper.IsTextMatchingVbcErrorPattern(text).Should().Be(expectedResult);
}
