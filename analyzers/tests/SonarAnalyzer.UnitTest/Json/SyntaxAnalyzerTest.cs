/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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

using System;
using System.Linq;
using System.Linq.Expressions;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Json;
using SonarAnalyzer.Json.Parsing;

namespace SonarAnalyzer.UnitTest.Common
{
    [TestClass]
    public class SyntaxAnalyzerTest
    {
        [DataTestMethod]
        [DataRow("[null]", null)]
        [DataRow("[true]", true)]
        [DataRow("[false]", false)]
        [DataRow("[42]", 42)]
        [DataRow("[42.42]", 42.42)]
        [DataRow("[\"Lorem Ipsum\"]", "Lorem Ipsum")]
        public void StandaloneValue(string source, object expected)
        {
            var sut = new SyntaxAnalyzer(source);
            var ret = sut.Parse();
            ret.Kind.Should().Be(Kind.List);
            ret.Should().HaveCount(1);
            var value = ret.Single();
            value.Kind.Should().Be(Kind.Value);
            value.Value.Should().Be(expected);
        }

        [DataTestMethod]
        [DataRow("{}")]
        [DataRow("{ }")]
        [DataRow(" { \t\n\r } ")]
        public void EmptyObject(string source)
        {
            var sut = new SyntaxAnalyzer(source);
            var ret = sut.Parse();
            ret.Kind.Should().Be(Kind.Object);
            ret.Count.Should().Be(0);
        }

        [DataTestMethod]
        [DataRow("[]")]
        [DataRow("[ ]")]
        [DataRow(" [ \t\n\r ] ")]
        public void EmptyList(string source)
        {
            var sut = new SyntaxAnalyzer(source);
            var ret = sut.Parse();
            ret.Kind.Should().Be(Kind.List);
            ret.Should().BeEmpty();
        }

        [TestMethod]
        public void ParseObject()
        {
            const string json = @"
{
    ""a"": ""aaa"",
    ""b"": 42,
    ""c"": true,
    ""d"": null
}";
            var sut = new SyntaxAnalyzer(json);
            var ret = sut.Parse();
            ret.Kind.Should().Be(Kind.Object);
            ret.ContainsKey("a").Should().BeTrue();
            ret.ContainsKey("b").Should().BeTrue();
            ret.ContainsKey("c").Should().BeTrue();
            ret.ContainsKey("d").Should().BeTrue();
            ret["a"].Value.Should().Be("aaa");
            ret["b"].Value.Should().Be(42);
            ret["c"].Value.Should().Be(true);
            ret["d"].Value.Should().Be(null);
        }

        [TestMethod]
        public void ParseList()
        {
            const string json = @"[""aaa"", 42, true, null]";
            var sut = new SyntaxAnalyzer(json);
            var ret = sut.Parse();
            ret.Kind.Should().Be(Kind.List);
            ret.Select(x => x.Value).Should().ContainInOrder("aaa", 42, true, null);
        }

        [TestMethod]
        public void ParseNested()
        {
            const string json = @"
{
    ""a"": [""aaa"", ""bbb"", ""ccc"", { ""x"": true}],
    ""b"": 42,
    ""c"": {""1"": 111, ""2"": ""222"", ""list"": [42, 43, 44]}
}";
            var sut = new SyntaxAnalyzer(json);
            var root = sut.Parse();
            root.Kind.Should().Be(Kind.Object);
            root.ContainsKey("a").Should().BeTrue();
            root.ContainsKey("b").Should().BeTrue();
            root.ContainsKey("c").Should().BeTrue();
            root.ContainsKey("d").Should().BeFalse();

            var a = root["a"];
            a.Kind.Should().Be(Kind.List);
            a.Where(x => x.Kind == Kind.Value).Select(x => x.Value).Should().ContainInOrder(new[] { "aaa", "bbb", "ccc" });
            var objectInList = a.Single(x => x.Kind == Kind.Object);
            objectInList.ContainsKey("x").Should().BeTrue();
            objectInList["x"].Value.Should().Be(true);

            root["b"].Value.Should().Be(42);

            var c = root["c"];
            c.Kind.Should().Be(Kind.Object);
            c.ContainsKey("1").Should().BeTrue();
            c.ContainsKey("2").Should().BeTrue();
            c.ContainsKey("list").Should().BeTrue();
            c["1"].Kind.Should().Be(Kind.Value);
            c["1"].Value.Should().Be(111);
            c["2"].Kind.Should().Be(Kind.Value);
            c["2"].Value.Should().Be("222");
            c["list"].Kind.Should().Be(Kind.List);
            c["list"].Select(x => x.Value).Should().ContainInOrder(42, 43, 44);
        }

        [DataTestMethod]
        [DataRow("true", "{ or [ expected, but Value found at line 1 position 1")]
        [DataRow(@"{ ""key"",", ": expected, but Comma found at line 1 position 8")]
        [DataRow("{,", "String Value expected, but Comma found at line 1 position 2")]
        [DataRow("[0 0", "] expected, but Value found at line 1 position 4")]
        [DataRow("[ ,", "{, [ or Value (true, false, null, String, Number) expected, but Comma found at line 1 position 3")]
        [DataRow(@"{ ""key"" : ""value"" ", "} expected, but EndOfInput found at line 1 position 11")]
        public void InvalidSyntax_Throws(string source, string expectedMessage)
        {
            var sut = new SyntaxAnalyzer(source);
            sut.Invoking(x => x.Parse()).Should().Throw<JsonException>().WithMessage(expectedMessage);
        }

        [TestMethod]
        public void Location()
        {
            const string json =
@"{
    'a': [
            'aaa',
            'bbb',
            'ccc',
            { 'x': true}
           ],
    'b': 42,
    'c': {
            '1': 111,
            '2': '222',
            'list': [42, 43, 44]
         },
    'd':
[]
}";
            var sut = new SyntaxAnalyzer(json.Replace('\'', '"'));  // Avoid "" escaping to preserve correct indexes in this editor
            var root = sut.Parse();
            AssertLocation(() => root, 0, 0, 15, 1);
            AssertLocation(() => root["a"], 1, 9, 6, 12);
            AssertLocation(() => root["b"], 7, 9, 7, 11);
            AssertLocation(() => root["c"], 8, 9, 12, 10);
            AssertLocation(() => root["d"], 14, 0, 14, 2);

            var array = root["a"];
            AssertLocation(() => array[0], 2, 12, 2, 17);
            AssertLocation(() => array[1], 3, 12, 3, 17);
            AssertLocation(() => array[2], 4, 12, 4, 17);
            AssertLocation(() => array[3], 5, 12, 5, 24);

            AssertLocation(() => array[3]["x"], 5, 19, 5, 23);
        }

        [TestMethod]
        public void Location_EndOfLines()
        {
            const string json = "[0,\n1,\r2,\r\n3,\u20284,\u20295]";
            var sut = new SyntaxAnalyzer(json);
            var root = sut.Parse();
            AssertLocation(() => root, 0, 0, 5, 2);
            AssertLocation(() => root[0], 0, 1, 0, 2);
            AssertLocation(() => root[1], 1, 0, 1, 1);
            AssertLocation(() => root[2], 2, 0, 2, 1);
            AssertLocation(() => root[3], 3, 0, 3, 1);
            AssertLocation(() => root[4], 4, 0, 4, 1);
            AssertLocation(() => root[5], 5, 0, 5, 1);
        }

        private static void AssertLocation(Expression<Func<JsonNode>> expression, int startLine, int startCharacter, int endLine, int endCharacter)
        {
            var node = expression.Compile()();
            node.Start.Line.Should().Be(startLine, expression.ToString());
            node.Start.Character.Should().Be(startCharacter, expression.ToString());
            node.End.Line.Should().Be(endLine, expression.ToString());
            node.End.Character.Should().Be(endCharacter, expression.ToString());
        }
    }
}
