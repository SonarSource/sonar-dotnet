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

using System.Reflection;
using Moq;

namespace SonarAnalyzer.Test.TestFramework.Tests.Common;

[TestClass]

public class CombinatorialDataAttributeTest_TwoDimensions
{
    private static List<(int X, int Y)> combinations;

    [ClassInitialize]
    public static void Initialize(TestContext context)
    {
        combinations = new();
    }

    [TestMethod]
    [CombinatorialData]
#pragma warning disable S2699 // Tests should include assertions. Assertion happens in cleanup
    public void Combinatorial([DataValues(1, 2, 3)] int x, [DataValues(-1, -2, -3)] int y)
#pragma warning restore S2699
    {
        combinations.Add((x, y));
    }

    [ClassCleanup]
    public static void Cleanup()
    {
        combinations.Should().BeEquivalentTo([
            (1, -1),
            (1, -2),
            (1, -3),
            (2, -1),
            (2, -2),
            (2, -3),
            (3, -1),
            (3, -2),
            (3, -3),
        ]);
    }
}

[TestClass]
public class CombinatorialDataAttributeTest_ThreeDimensions
{
    private static List<(int X, string Y, bool Z)> combinations;

    [ClassInitialize]
    public static void Initialize(TestContext context)
    {
        combinations = new();
    }

    [TestMethod]
    [CombinatorialData]
#pragma warning disable S2699 // Tests should include assertions. Assertion happens in cleanup
    public void Combinatorial([DataValues(1, 2, 3)] int x, [DataValues("A", "B")] string y, [DataValues(true, false)] bool z)
#pragma warning restore S2699
    {
        combinations.Add((x, y, z));
    }

    [ClassCleanup]
    public static void Cleanup()
    {
        combinations.Should().BeEquivalentTo([
            (1, "A", true),
            (1, "B", true),
            (1, "A", false),
            (1, "B", false),
            (2, "A", true),
            (2, "B", true),
            (2, "A", false),
            (2, "B", false),
            (3, "A", true),
            (3, "B", true),
            (3, "A", false),
            (3, "B", false),
        ]);
    }
}

[TestClass]
public class CombinatorialDataAttributeTest_AttributeTest
{
    [TestMethod]
    public void Combinatorial_Test()
    {
        var attribute = new CombinatorialDataAttribute();
        var data = attribute.GetData(typeof(CombinatorialDataAttributeTest_AttributeTest).GetMethod(nameof(Combinatorial)));
        data.Should().BeEquivalentTo<object[]>([
            [1, "A", true],
            [1, "B", true],
            [1, "A", false],
            [1, "B", false],
            [2, "A", true],
            [2, "B", true],
            [2, "A", false],
            [2, "B", false],
            [3, "A", true],
            [3, "B", true],
            [3, "A", false],
            [3, "B", false],
            ]);
    }

    [TestMethod]
    public void MissingAttribute_Test()
    {
        var attribute = new CombinatorialDataAttribute();
        var data = () => attribute.GetData(typeof(CombinatorialDataAttributeTest_AttributeTest).GetMethod(nameof(MissingAttribute))).ToList();
        data.Should().Throw<InvalidOperationException>().WithMessage("Combinatorial test requires all parameters to have the [DataValues] attribute set");
    }

    [DataTestMethod]
    [DataRow("Test ()")]
    [DataRow("Test ()", null)]
    [DataRow("Test (a)", "a")]
    [DataRow("Test (a,b)", "a", "b")]
    [DataRow("Test (a,b,1)", "a", "b", 1)]
    public void GetDisplayName_Test(string expected, params object[] arguments)
    {
        var attribute = new CombinatorialDataAttribute();
        var methodInfo = new Mock<MethodInfo>();
        methodInfo.SetupGet(x => x.Name).Returns("Test");
        var actual = attribute.GetDisplayName(methodInfo.Object, arguments);
        actual.Should().Be(expected);
    }

    [TestMethod]
    public void GetDisplayName_Null()
    {
        var attribute = new CombinatorialDataAttribute();
        var methodInfo = new Mock<MethodInfo>();
        methodInfo.SetupGet(x => x.Name).Returns("Test");
        var actual = attribute.GetDisplayName(methodInfo.Object, null);
        actual.Should().Be("Test ()");
    }

    public static void Combinatorial([DataValues(1, 2, 3)] int x, [DataValues("A", "B")] string y, [DataValues(true, false)] bool z)
    {
        // Used for reflection only.
    }

    public static void MissingAttribute([DataValues(1, 2, 3)] int x, string y, [DataValues(true, false)] bool z)
    {
        // Used for reflection only.
    }
}
