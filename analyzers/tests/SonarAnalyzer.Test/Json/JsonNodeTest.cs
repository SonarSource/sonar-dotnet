﻿/*
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

using System.Collections;
using Microsoft.CodeAnalysis.Text;
using SonarAnalyzer.Json;
using SonarAnalyzer.Json.Parsing;

namespace SonarAnalyzer.Test.Common
{
    [TestClass]
    public class JsonNodeTest
    {
        [TestMethod]
        public void BehavesAsValue()
        {
            var sut = new JsonNode(LinePosition.Zero, LinePosition.Zero, 42);
            sut.Kind.Should().Be(Kind.Value);
            sut.Value.Should().Be(42);
            sut.Invoking(x => x.Count).Should().Throw<InvalidOperationException>();
            sut.Invoking(x => x[0]).Should().Throw<InvalidOperationException>();
            sut.Invoking(x => x["Key"]).Should().Throw<InvalidOperationException>();
            sut.Invoking(x => x.Keys).Should().Throw<InvalidOperationException>();
            sut.Invoking(x => x.Add(sut)).Should().Throw<InvalidOperationException>();
            sut.Invoking(x => x.Add("Key", sut)).Should().Throw<InvalidOperationException>();
            sut.Invoking(x => x.ContainsKey("Key")).Should().Throw<InvalidOperationException>();
            sut.Invoking(x => ((IEnumerable)x).GetEnumerator()).Should().Throw<InvalidOperationException>();
        }

        [TestMethod]
        public void UnsupportedKinds()
        {
            Func<JsonNode> action = () => new JsonNode(LinePosition.Zero, Kind.Value);
            action.Should().Throw<InvalidOperationException>();
            action = () => new JsonNode(LinePosition.Zero, Kind.Unknown);
            action.Should().Throw<InvalidOperationException>();
        }

        [TestMethod]
        public void BehavesAsList()
        {
            var a = new JsonNode(LinePosition.Zero, LinePosition.Zero, "a");
            var b = new JsonNode(LinePosition.Zero, LinePosition.Zero, "b");
            var sut = new JsonNode(LinePosition.Zero, Kind.List);
            sut.Add(a);
            sut.Add(b);
            sut.Kind.Should().Be(Kind.List);
            sut.Should().HaveCount(2);
            ((object)sut[0]).Should().Be(a);
            ((object)sut[1]).Should().Be(b);
            var cnt = 0;
            foreach (var item in sut)
            {
                new[] { a, b }.Should().Contain(item);
                cnt++;
            }
            cnt.Should().Be(2);
            sut.Invoking(x => x.Value).Should().Throw<InvalidOperationException>();
            sut.Invoking(x => x["Key"]).Should().Throw<InvalidOperationException>();
            sut.Invoking(x => x.Keys).Should().Throw<InvalidOperationException>();
            sut.Invoking(x => x.Add("Key", sut)).Should().Throw<InvalidOperationException>();
            sut.Invoking(x => x.ContainsKey("Key")).Should().Throw<InvalidOperationException>();
        }

        [TestMethod]
        public void BehavesAsDictionary()
        {
            var a = new JsonNode(LinePosition.Zero, LinePosition.Zero, "a");
            var b = new JsonNode(LinePosition.Zero, LinePosition.Zero, "b");
            var sut = new JsonNode(LinePosition.Zero, Kind.Object);
            sut.Add("KeyA", a);
            sut.Add("KeyB", b);
            sut.Kind.Should().Be(Kind.Object);
            sut.Count.Should().Be(2);
            ((object)sut["KeyA"]).Should().Be(a);
            ((object)sut["KeyB"]).Should().Be(b);
            sut.Keys.Should().BeEquivalentTo("KeyA", "KeyB");
            sut.ContainsKey("KeyA").Should().BeTrue();
            sut.ContainsKey("KeyB").Should().BeTrue();
            sut.ContainsKey("KeyC").Should().BeFalse();
            sut.Invoking(x => x.Value).Should().Throw<InvalidOperationException>();
            sut.Invoking(x => x[0]).Should().Throw<InvalidOperationException>();
            sut.Invoking(x => x.Add(sut)).Should().Throw<InvalidOperationException>();
            sut.Invoking(x => ((IEnumerable)x).GetEnumerator()).Should().Throw<InvalidOperationException>();
        }

        [TestMethod]
        public void UpdateEnd()
        {
            var start = new LinePosition(1, 42);
            var end = new LinePosition(2, 10);
            var sut = new JsonNode(start, Kind.List);
            sut.Start.Should().Be(start);
            sut.End.Should().Be(LinePosition.Zero);

            sut.UpdateEnd(end);
            sut.End.Should().Be(end);

            sut.Invoking(x => x.UpdateEnd(end)).Should().Throw<InvalidOperationException>();
        }

        // Light-weight way to test that string could be parsed. Precise tests could be found in SyntaxAnalyzerTest.cs
        [TestMethod]
        public void ParsedFromString()
        {
            var sut = JsonNode.FromString(@"[""a"",""b""]");
            sut.Kind.Should().Be(Kind.List);
            sut.Should().HaveCount(2);
            sut[0].Kind.Should().Be(Kind.Value);
            sut[1].Kind.Should().Be(Kind.Value);
            sut[0].Value.Should().Be("a");
            sut[1].Value.Should().Be("b");
        }
    }
}
