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

using SonarAnalyzer.Core.Rules.Utilities;
using SonarAnalyzer.Protobuf;

namespace SonarAnalyzer.Test.Rules.Utilities;

[TestClass]
public class MethodDeclarationInfoComparerTest
{
    [DataTestMethod]
    [DataRow("type", "method", "type", "method", true)]
    [DataRow("Type", "method", "type", "method", false)] // Case-sensitive
    [DataRow("type", "Method", "type", "method", false)] // Case-sensitive
    [DataRow("type", "method", "type", "method2", false)]
    [DataRow("type", "method", "type2", "method", false)]
    public void Equals(string firstType, string firstMethod, string secondType, string secondMethod, bool expected)
    {
        var first = new MethodDeclarationInfo { TypeName = firstType, MethodName = firstMethod };
        var second = new MethodDeclarationInfo { TypeName = secondType, MethodName = secondMethod };
        var sut = new MethodDeclarationInfoComparer();

        sut.Equals(first, second).Should().Be(expected);
    }

    [TestMethod]
    public void Equals_Null()
    {
        var sut = new MethodDeclarationInfoComparer();

        sut.Equals(null, null).Should().BeTrue();
        sut.Equals(new MethodDeclarationInfo(), null).Should().BeFalse();
        sut.Equals(null, new MethodDeclarationInfo()).Should().BeFalse();
    }

    [TestMethod]
    public void GetHashCode_EqualForObjectsWithTheSameProperties()
    {
        var first = new MethodDeclarationInfo { TypeName = "type", MethodName = "method" };
        var second = new MethodDeclarationInfo { TypeName = "type", MethodName = "method" };
        var sut = new MethodDeclarationInfoComparer();

        sut.GetHashCode(first).Should().Be(sut.GetHashCode(second));
    }

    [TestMethod]
    public void GetHashCode_DifferentForObjectsWithDifferentProperties()
    {
        var first = new MethodDeclarationInfo { TypeName = "type", MethodName = "method" };
        var second = new MethodDeclarationInfo { TypeName = "different type", MethodName = "method" };
        var third = new MethodDeclarationInfo { TypeName = "different type", MethodName = "different method" };
        var sut = new MethodDeclarationInfoComparer();

        sut.GetHashCode(first).Should().NotBe(sut.GetHashCode(second));
        sut.GetHashCode(first).Should().NotBe(sut.GetHashCode(third));
    }
}
