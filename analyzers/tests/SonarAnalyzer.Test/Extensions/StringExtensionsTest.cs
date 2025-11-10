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

using SonarAnalyzer.CFG.Extensions;

namespace SonarAnalyzer.Test.Extensions;

[TestClass]
public class StringExtensionsTest
{
    [TestMethod]
    public void TestSplitCamelCaseToWords()
    {
        AssertSplitEquivalent("thisIsAName", "THIS", "IS", "A", "NAME");
        AssertSplitEquivalent("thisIsSMTPName", "THIS", "IS", "SMTP", "NAME");
        AssertSplitEquivalent("ThisIsIt", "THIS", "IS", "IT");
        AssertSplitEquivalent("bin2hex", "BIN", "HEX");
        AssertSplitEquivalent("HTML", "HTML");
        AssertSplitEquivalent("SOME_VALUE", "SOME", "VALUE");
        AssertSplitEquivalent("GR8day", "GR", "DAY");
        AssertSplitEquivalent("ThisIsEpic", "THIS", "IS", "EPIC");
        AssertSplitEquivalent("ThisIsEPIC", "THIS", "IS", "EPIC");
        AssertSplitEquivalent("This_is_EPIC", "THIS", "IS", "EPIC");
        AssertSplitEquivalent("PEHeader", "PE", "HEADER");
        AssertSplitEquivalent("PE_Header", "PE", "HEADER");
        AssertSplitEquivalent("BigB_smallc&GIANTD", "BIG", "B", "SMALLC", "GIANTD");
        AssertSplitEquivalent("SMTPServer", "SMTP", "SERVER");
        AssertSplitEquivalent("__url_foo", "URL", "FOO");
        AssertSplitEquivalent("");
        AssertSplitEquivalent(null);
    }

    private static void AssertSplitEquivalent(string name, params string[] words) =>
        CollectionAssert.AreEquivalent(words, name.SplitCamelCaseToWords().ToList(), $" Value: {name}");
}
