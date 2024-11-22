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

namespace SonarAnalyzer.Core.Test.Common;

[TestClass]
public class BidirectionalDictionaryTest
{
    private BidirectionalDictionary<int, int> sut;

    [TestInitialize]
    public void Initialize() =>
        sut = new BidirectionalDictionary<int, int>();

    [TestMethod]
    public void AddKeyA_AlreadyExists_Throws()
    {
        sut.Add(1, 2);
        sut.Invoking(x => x.Add(1, 3)).Should().Throw<ArgumentException>().WithMessage("An element with the same key already exists in the BidirectionalDictionary.");
    }

    [TestMethod]
    public void AddKeyB_AlreadyExists_Throws()
    {
        sut.Add(1, 2);
        sut.Invoking(x => x.Add(3, 2)).Should().Throw<ArgumentException>().WithMessage("An element with the same key already exists in the BidirectionalDictionary.");
    }

    [TestMethod]
    public void Keys_ReturnsCorrectKeys()
    {
        sut.Add(1, 11);
        sut.Add(2, 12);
        sut.Add(3, 13);
        sut.Add(4, 14);
        sut.Add(5, 15);

        sut.AKeys.Should().BeEquivalentTo(new[] {1, 2, 3, 4, 5});
        sut.BKeys.Should().BeEquivalentTo(new[] {11, 12, 13, 14, 15});
    }

    [TestMethod]
    public void GetByA_ElementExists_ReturnsCorrectElement()
    {
        sut.Add(1, 2);
        sut.GetByA(1).Should().Be(2);
    }

    [TestMethod]
    public void GetByB_ElementExists_ReturnsCorrectElement()
    {
        sut.Add(1, 2);
        sut.GetByB(2).Should().Be(1);
    }

    [TestMethod]
    public void GetByA_ElementDoesNotExist_ThrowsException() =>
        sut.Invoking(x => x.GetByA(1)).Should().Throw<KeyNotFoundException>();

    [TestMethod]
    public void GetByB_ElementDoesNotExist_ThrowsException() =>
        sut.Invoking(x => x.GetByB(1)).Should().Throw<KeyNotFoundException>();

    [TestMethod]
    public void ContainsKeyByA_ElementExists_ReturnsTrue()
    {
        sut.Add(4, 2);
        sut.ContainsKeyByA(4).Should().BeTrue();
    }

    [TestMethod]
    public void ContainsKeyByB_ElementExists_ReturnsTrue()
    {
        sut.Add(4, 2);
        sut.ContainsKeyByB(2).Should().BeTrue();
    }

    [TestMethod]
    public void ContainsKeyByA_ElementDoesNotExist_ReturnsFalse()
    {
        sut.Add(2, 3);
        sut.ContainsKeyByA(1).Should().BeFalse();
    }

    [TestMethod]
    public void ContainsKeyByB_ElementDoesNotExist_ReturnsFalse()
    {
        sut.Add(2, 3);
        sut.ContainsKeyByB(1).Should().BeFalse();
    }
}
