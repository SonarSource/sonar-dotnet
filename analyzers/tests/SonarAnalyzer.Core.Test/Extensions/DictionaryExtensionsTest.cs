/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

namespace SonarAnalyzer.Core.Test.Extensions;

[TestClass]
public class DictionaryExtensionsTest
{
    [TestMethod]
    public void DictionaryEquals_Different()
    {
        var empty = new Dictionary<string, string>();
        var original = new Dictionary<string, string> { { "a", "a" }, { "b", "b" } };
        var differentKeys = new Dictionary<string, string> { { "a", "a" }, { "c", "c" } };
        var differentValues = new Dictionary<string, string> { { "a", "a" }, { "b", "xxxx" } };

        DictionaryExtensions.DictionaryEquals(null, empty).Should().BeFalse();
        DictionaryExtensions.DictionaryEquals(empty, null).Should().BeFalse();
        original.DictionaryEquals(empty).Should().BeFalse();
        original.DictionaryEquals(differentKeys).Should().BeFalse();
        original.DictionaryEquals(differentValues).Should().BeFalse();
    }

    [TestMethod]
    public void DictionaryEquals_SameContent()
    {
        var dict1 = new Dictionary<string, string> { { "a", "a" }, { "b", "b" } };
        var dict2 = new Dictionary<string, string> { { "a", "a" }, { "b", "b" } };
        dict1.DictionaryEquals(dict1).Should().BeTrue();
        dict1.DictionaryEquals(dict2).Should().BeTrue();
    }

    [TestMethod]
    public void DictionaryEquals_SameContent_DifferentOrdering()
    {
        var numbers = Enumerable.Range(1, 1000);
        var dict1 = numbers.ToDictionary(x => x, x => x);
        var dict2 = numbers.OrderByDescending(x => x).ToDictionary(x => x, x => x);
        dict1.DictionaryEquals(dict1).Should().BeTrue();
        dict1.DictionaryEquals(dict2).Should().BeTrue();
        dict2.DictionaryEquals(dict1).Should().BeTrue();
    }
}
