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

#pragma warning disable CA1825 // Avoid zero-length array allocations

using HashCode = SonarAnalyzer.Core.Common.HashCode;

namespace SonarAnalyzer.Core.Common.Test;

[TestClass]
public class HashCodeTest
{
    [TestMethod]
    [DataRow(null)]
    [DataRow("Lorem Ipsum")]
    public void Combine_ProducesDifferentResults(string input)
    {
        var hash2 = HashCode.Combine(input, input);
        var hash3 = HashCode.Combine(input, input, input);
        var hash4 = HashCode.Combine(input, input, input, input);
        var hash5 = HashCode.Combine(input, input, input, input, input);

        hash2.Should().NotBe(0);
        hash3.Should().NotBe(0).And.NotBe(hash2);
        hash4.Should().NotBe(0).And.NotBe(hash2).And.NotBe(hash3);
        hash5.Should().NotBe(0).And.NotBe(hash2).And.NotBe(hash3).And.NotBe(hash4);
    }

    [TestMethod]
    public void DictionaryContentHash_StableForImmutableDictionary()
    {
        var numbers = Enumerable.Range(1, 1000);
        var dict1 = numbers.ToImmutableDictionary(x => x, x => x);
        var dict2 = numbers.OrderByDescending(x => x).ToImmutableDictionary(x => x, x => x);
        var hashCode1 = HashCode.DictionaryContentHash(dict1);
        var hashCode2 = HashCode.DictionaryContentHash(dict2);
        hashCode1.Should().Be(hashCode2);
    }

    [TestMethod]
    public void EnumerableUnorderedContentHash_Empty()
    {
        var ints = new int[0];
        var strings = new string[0];

        HashCode.EnumerableUnorderedContentHash(ints).Should().Be(HashCode.EnumerableUnorderedContentHash(new int[0]));
        HashCode.EnumerableUnorderedContentHash(strings).Should().Be(HashCode.EnumerableUnorderedContentHash(strings));
        HashCode.EnumerableUnorderedContentHash(ints).Should().Be(HashCode.EnumerableUnorderedContentHash(strings));
    }

    [TestMethod]
    public void EnumerableUnorderedContentHash_Order()
    {
        var ints1 = new[] { 0, 1, 2 };
        var ints2 = new[] { 2, 1, 0 };
        var ints3 = new[] { 0, 1, 8 };

        HashCode.EnumerableUnorderedContentHash(ints1).Should().Be(HashCode.EnumerableUnorderedContentHash(ints2)).And.NotBe(0);
        HashCode.EnumerableUnorderedContentHash(ints1).Should().NotBe(HashCode.EnumerableUnorderedContentHash(ints3));
    }

    [TestMethod]
    public void EnumerableUnorderedContentHash_DifferentLength()
    {
        var ints1 = new[] { 0, 1, 2 };
        var ints2 = new[] { 0, 1, 2, 3 };

        HashCode.EnumerableUnorderedContentHash(ints1).Should().NotBe(HashCode.EnumerableUnorderedContentHash(ints2));
    }

    [TestMethod]
    public void EnumerableOrderedContentHash_Empty()
    {
        var ints = new int[0];
        var strings = new string[0];

        HashCode.EnumerableOrderedContentHash(ints).Should().Be(HashCode.EnumerableOrderedContentHash(new int[0]));
        HashCode.EnumerableOrderedContentHash(strings).Should().Be(HashCode.EnumerableOrderedContentHash(strings));
        HashCode.EnumerableOrderedContentHash(ints).Should().Be(HashCode.EnumerableOrderedContentHash(strings));
    }

    [TestMethod]
    public void EnumerableOrderedContentHash_Order()
    {
        var ints1 = new[] { 0, 1, 2 };
        var ints2 = new[] { 0, 1, 2 };
        var ints3 = new[] { 2, 1, 0 };
        var ints4 = new[] { 0, 1, 8 };

        HashCode.EnumerableOrderedContentHash(ints1).Should().Be(HashCode.EnumerableOrderedContentHash(ints2)).And.NotBe(0);
        HashCode.EnumerableOrderedContentHash(ints1).Should().NotBe(HashCode.EnumerableOrderedContentHash(ints3));
        HashCode.EnumerableOrderedContentHash(ints1).Should().NotBe(HashCode.EnumerableOrderedContentHash(ints4));
    }

    [TestMethod]
    public void EnumerableOrderedContentHash_DifferentLength()
    {
        var ints1 = new[] { 0, 1, 2 };
        var ints2 = new[] { 0, 1, 2, 3 };

        HashCode.EnumerableOrderedContentHash(ints1).Should().NotBe(HashCode.EnumerableOrderedContentHash(ints2));
    }
}
