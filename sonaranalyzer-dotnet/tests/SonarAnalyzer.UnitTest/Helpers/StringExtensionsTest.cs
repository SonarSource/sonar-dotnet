/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
 * mailto: contact AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.UnitTest.Helpers
{
    [TestClass]
    public class StringExtensionsTest
    {
        [TestMethod]
        public void TestSplitCamelCaseToWords()
        {
            AssertSplitEquivalent("thisIsAName", "THIS", "IS", "A", "NAME");
            AssertSplitEquivalent("ThisIsIt", "THIS", "IS", "IT");
            AssertSplitEquivalent("bin2hex", "BIN", "HEX");
            AssertSplitEquivalent("HTML", "H", "T", "M", "L");
            AssertSplitEquivalent("PEHeader", "P", "E", "HEADER");
            AssertSplitEquivalent("__url_foo", "URL", "FOO");
            AssertSplitEquivalent("");
            AssertSplitEquivalent(null);
        }

        private void AssertSplitEquivalent(string name, params string[] words)
        {
            CollectionAssert.AreEquivalent(name.SplitCamelCaseToWords().ToList(), words);
        }
    }
}
