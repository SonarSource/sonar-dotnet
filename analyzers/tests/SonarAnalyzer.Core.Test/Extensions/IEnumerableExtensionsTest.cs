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

#pragma warning disable SA1122 // Use string.Empty for empty strings

namespace SonarAnalyzer.Core.Test.Extensions;

[TestClass]
public class IEnumerableExtensionsTest
{
    [TestMethod]
    public void TestAreEqual_01()
    {
        var c1 = new List<int> { 1, 2, 3 };
        var c2 = new List<string> { "1", "2", "3" };

        c1.Equals(c2, (e1, e2) => e1.ToString() == e2).Should().BeTrue();
    }

    [TestMethod]
    public void TestAreEqual_02()
    {
        var c1 = new List<int> { 1, 2 };
        var c2 = new List<string> { "1", "2", "3" };

        c1.Equals(c2, (e1, e2) => e1.ToString() == e2).Should().BeFalse();
    }

    [TestMethod]
    public void TestAreEqual_03()
    {
        var c1 = new List<int> { 1, 2, 3 };
        var c2 = new List<string> { "1", "2" };

        c1.Equals(c2, (e1, e2) => e1.ToString() == e2).Should().BeFalse();
    }

    [TestMethod]
    public void TestAreEqual_04()
    {
        var c1 = new List<int>();
        var c2 = new List<string> { "1", "2" };

        c1.Equals(c2, (e1, e2) => e1.ToString() == e2).Should().BeFalse();
    }

    [TestMethod]
    public void TestAreEqual_05()
    {
        var c1 = new List<int> { 1 };
        var c2 = new List<string>();

        c1.Equals(c2, (e1, e2) => e1.ToString() == e2).Should().BeFalse();
    }

    [TestMethod]
    public void TestAreEqual_06()
    {
        var c1 = new List<int>();
        var c2 = new List<string>();

        c1.Equals(c2, (e1, e2) => e1.ToString() == e2).Should().BeTrue();
    }

    [TestMethod]
    public void JoinStr_T_String()
    {
        var lst = new[]
        {
            Tuple.Create(1, "a"),
            Tuple.Create(2, "bb"),
            Tuple.Create(3, "ccc")
        };

        lst.JoinStr(null, x => x.Item2).Should().Be("abbccc");
        lst.JoinStr(", ", x => x.Item2).Should().Be("a, bb, ccc");
        lst.JoinStr(" ", x => x.Item2 + "!").Should().Be("a! bb! ccc!");
        lst.JoinStr("; ", x => x.Item1 + ":" + x.Item2).Should().Be("1:a; 2:bb; 3:ccc");
    }

    [TestMethod]
    public void JoinStr_T_Int()
    {
        var lst = new[]
        {
            Tuple.Create(1, "a"),
            Tuple.Create(2, "bb"),
            Tuple.Create(3, "ccc")
        };

        lst.JoinStr(", ", x => x.Item1).Should().Be("1, 2, 3");
        lst.JoinStr(null, x => x.Item1 + 10).Should().Be("111213");
    }

    [TestMethod]
    public void JoinStr_String()
    {
        Array.Empty<string>().JoinStr(", ").Should().Be("");
        new[] { "a" }.JoinStr(", ").Should().Be("a");
        new[] { "a", "bb", "ccc" }.JoinStr(", ").Should().Be("a, bb, ccc");
        new[] { "a", "bb", "ccc" }.JoinStr(null).Should().Be("abbccc");
    }

    [TestMethod]
    public void JoinStr_Int()
    {
        Array.Empty<int>().JoinStr(", ").Should().Be("");
        new[] { 1 }.JoinStr(", ").Should().Be("1");
        new[] { 1, 22, 333 }.JoinStr(", ").Should().Be("1, 22, 333");
        new[] { 1, 22, 333 }.JoinStr(null).Should().Be("122333");
    }

    [TestMethod]
    public void JoinNonEmpty()
    {
        Array.Empty<string>().JoinNonEmpty(", ").Should().Be("");
        new[] { "a" }.JoinNonEmpty(", ").Should().Be("a");
        new[] { "a", "bb", "ccc" }.JoinNonEmpty(", ").Should().Be("a, bb, ccc");
        new[] { "a", "bb", "ccc" }.JoinNonEmpty(null).Should().Be("abbccc");
        new[] { "a", "bb", "ccc" }.JoinNonEmpty("").Should().Be("abbccc");
        new[] { null, "a", "b" }.JoinNonEmpty(".").Should().Be("a.b");
        new[] { "a", null, "b" }.JoinNonEmpty(".").Should().Be("a.b");
        new[] { "a", "b", null }.JoinNonEmpty(".").Should().Be("a.b");
        new string[] { null, null, null }.JoinNonEmpty(".").Should().Be("");
        new string[] { "", "", "" }.JoinNonEmpty(".").Should().Be("");
        new string[] { "", "\t", " " }.JoinNonEmpty(".").Should().Be("");
        new string[] { "a", "\t", "b" }.JoinNonEmpty(".").Should().Be("a.b");
    }

    [TestMethod]
    public void WhereNotNull_Class()
    {
        var instance = new object();
        Array.Empty<object>().WhereNotNull().Should().BeEmpty();
        new object[] { null, null, null }.WhereNotNull().Should().BeEmpty();
        new object[] { 1, "a", instance }.WhereNotNull().Should().BeEquivalentTo(new object[] { 1, "a", instance });
        new object[] { 1, "a", null }.WhereNotNull().Should().BeEquivalentTo(new object[] { 1, "a" });
    }

    [TestMethod]
    public void WhereNotNull_NullableStruct()
    {
        Array.Empty<StructType?>().WhereNotNull().Should().BeEmpty();
        new StructType?[] { null, null, null }.WhereNotNull().Should().BeEmpty();
        new StructType?[] { new StructType(1), new StructType(2), new StructType(3) }
                          .WhereNotNull().Should().BeEquivalentTo(new object[] { new StructType(1), new StructType(2), new StructType(3) });
        new StructType?[] { new StructType(1), new StructType(2), null }.WhereNotNull().Should().BeEquivalentTo(new object[] { new StructType(1), new StructType(2) });
    }

    [TestMethod]
    public void JoinAndNull() =>
        ((object[])null).JoinAnd().Should().Be(string.Empty);

    [DataTestMethod]
    [DataRow("")] // empty collection
    [DataRow("", null)]
    [DataRow("", "")]
    [DataRow("", "", "")]
    [DataRow("", "", " ", "\t", null)]
    [DataRow("a", "a")]
    [DataRow("a and b", "a", "b")]
    [DataRow("a and b", "a", null, "b")]
    [DataRow("a, b, and c", "a", "b", "c")]
    [DataRow("a, b, and c", "a", null, "b", "", "c")]
    public void JoinAndStrings(string expected, params string[] collection) =>
        collection.JoinAnd().Should().Be(expected);

    [DataTestMethod]
    [DataRow("")] // Empty collection
    [DataRow("0", 0)]
    [DataRow("0 and 1", 0, 1)]
    [DataRow("0, 1, and 2", 0, 1, 2)]
    [DataRow("1000", 1000)]
    public void JoinAndInts(string expected, params int[] collection) =>
        collection.JoinAnd().Should().Be(expected);

    [DataTestMethod]
    [DataRow("")] // Empty collection
    [DataRow("08/30/2022 12:29:11", "2022-08-30T12:29:11")]
    [DataRow("08/30/2022 12:29:11 and 12/24/2022 16:00:00", "2022-08-30T12:29:11", "2022-12-24T16:00:00")]
    [DataRow("08/30/2022 12:29:11, 12/24/2022 16:00:00, and 12/31/2022 00:00:00", "2022-08-30T12:29:11", "2022-12-24T16:00:00", "2022-12-31T00:00:00")]
    public void JoinAndDateTime(string expected, params string[] collection)
    {
        using var scope = new CurrentCultureScope();
        collection.Select(x => DateTime.Parse(x)).JoinAnd().Should().Be(expected);
    }

    [TestMethod]
    public void JoinAndMixedClasses()
    {
        var collection = new Exception[]
        {
            new IndexOutOfRangeException("IndexOutOfRangeMessage"),
            new InvalidOperationException("OperationMessage"),
            null,
            new NotSupportedException("NotSupportedMessage"),
        };
        var result = collection.JoinAnd();
        result.Should().Be("System.IndexOutOfRangeException: IndexOutOfRangeMessage, System.InvalidOperationException: OperationMessage, and System.NotSupportedException: NotSupportedMessage");
    }

    private struct StructType
    {
        private readonly int count;

        public StructType(int count)
        {
            this.count = count;
        }
    }
}

#pragma warning restore SA1122 // Use string.Empty for empty strings
