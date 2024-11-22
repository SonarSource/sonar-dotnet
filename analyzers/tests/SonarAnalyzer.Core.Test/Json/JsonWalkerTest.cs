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

using SonarAnalyzer.Json;

namespace SonarAnalyzer.Core.Test.Json;

[TestClass]
public class JsonWalkerTest
{
    [TestMethod]
    public void VisitsAllNodes()
    {
        const string json = @"
{
    ""OuterKey"": ""OuterValue"",
    ""OuterBool"": true,
    ""OuterNull"": null,
    ""NestedArray"": [
        ""Array1"",
        [""Array2-Nested1"", null, ""Array2-Nested2"", { ""InnerKey"": ""Array2-NestedObject"" }],
        ""Array3""
    ]
}";
        var sut = new JsonWalkerCollector();
        sut.Visit(JsonNode.FromString(json));
        sut.VisitedKeys.Should().BeEquivalentTo("OuterKey", "OuterBool", "OuterNull", "NestedArray", "InnerKey");
        sut.VisitedValues.Should().BeEquivalentTo(new object[] { "OuterValue", true, null, "Array1", "Array2-Nested1", null, "Array2-Nested2", "Array2-NestedObject", "Array3" });
    }

    [DataTestMethod]
    [DataRow("[]")]
    [DataRow("{}")]
    public void VisitsAtomicJson_VisitsEmpty(string json)
    {
        var sut = new JsonWalkerCollector();
        sut.Visit(JsonNode.FromString(json));
        sut.VisitedKeys.Should().BeEmpty();
        sut.VisitedValues.Should().BeEmpty();
    }

    private class JsonWalkerCollector : JsonWalker
    {
        public readonly List<string> VisitedKeys = new();
        public readonly List<object> VisitedValues = new();

        protected override void VisitObject(string key, JsonNode value)
        {
            VisitedKeys.Add(key);
            base.VisitObject(key, value);
        }

        protected override void VisitValue(JsonNode node)
        {
            VisitedValues.Add(node.Value);
            base.VisitValue(node);
        }
    }
}
