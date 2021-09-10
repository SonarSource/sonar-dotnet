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
using System.Collections;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Text;
using SonarAnalyzer.Json.Parsing;

namespace SonarAnalyzer.Json
{
    public sealed class JsonNode : IEnumerable<JsonNode>
    {
        public Kind Kind { get; }

        private readonly List<JsonNode> list;
        private readonly Dictionary<string, JsonNode> map;
        private readonly object value;

        public LinePosition Start { get; }
        public LinePosition End { get; private set; }
        public JsonNode this[int index] => Kind == Kind.List ? list[index] : throw InvalidKind();
        public JsonNode this[string key] => Kind == Kind.Object ? map[key] : throw InvalidKind();
        public IEnumerable<string> Keys => Kind == Kind.Object ? map.Keys : throw InvalidKind();
        public object Value => Kind == Kind.Value ? value : throw InvalidKind();
        public int Count => Kind switch
        {
            Kind.Object => map.Count,
            Kind.List => list.Count,
            _ => throw InvalidKind()
        };

        public JsonNode(LinePosition start, LinePosition end, object value)
        {
            Kind = Kind.Value;
            Start = start;
            End = end;
            this.value = value;
        }

        public JsonNode(LinePosition start, Kind kind)
        {
            Kind = kind;
            Start = start;
            switch (kind)
            {
                case Kind.List:
                    list = new List<JsonNode>();
                    break;
                case Kind.Object:
                    map = new Dictionary<string, JsonNode>();
                    break;
                default:
                    throw InvalidKind();
            }
        }

        public static JsonNode FromString(string json) =>
            new SyntaxAnalyzer(json).Parse();

        public void UpdateEnd(LinePosition end) =>
            End = End == LinePosition.Zero
                ? end
                : throw new InvalidOperationException("End position is already set");

        public void Add(JsonNode value)
        {
            if (Kind == Kind.List)
            {
                list.Add(value);
            }
            else
            {
                throw InvalidKind();
            }
        }

        public void Add(string key, JsonNode value) =>
            map[key] = Kind == Kind.Object ? value : throw InvalidKind();

        public bool ContainsKey(string key) =>
            Kind == Kind.Object ? map.ContainsKey(key) : throw InvalidKind();

        public IEnumerator<JsonNode> GetEnumerator() =>
            Kind == Kind.List ? list.GetEnumerator() : throw InvalidKind();

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();

        private InvalidOperationException InvalidKind() =>
            new InvalidOperationException("Operation is not valid. Json kind is " + Kind);
    }
}
