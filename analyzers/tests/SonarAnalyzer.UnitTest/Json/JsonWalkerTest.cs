﻿/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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

using SonarAnalyzer.Json;

namespace SonarAnalyzer.UnitTest.Common
{
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
    ""NestedArray"": [
        ""Array1"",
        ""Array2"",
        ""Array3""
    ]
}";
            var sut = new JsonWalkerCollector();
            sut.Visit(JsonNode.FromString(json));
            sut.VisitedKeys.Should().BeEquivalentTo("OuterKey", "OuterBool", "NestedArray");
            sut.VisitedValues.Should().BeEquivalentTo("OuterValue", true, "Array1", "Array2", "Array3");
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
}
