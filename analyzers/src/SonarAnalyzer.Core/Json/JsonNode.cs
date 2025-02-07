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

using System.Collections;
using Microsoft.CodeAnalysis.Text;
using SonarAnalyzer.Json.Parsing;

namespace SonarAnalyzer.Json;

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

    public static JsonNode FromString(string json)
    {
        try
        {
            return new SyntaxAnalyzer(json).Parse();
        }
        catch (JsonException)
        {
            return null;    // Malformed Json
        }
    }

    public Location ToLocation(string path)
    {
        var length = Value.ToString().Length;
        var start = new LinePosition(Start.Line, Start.Character + 1);
        var end = new LinePosition(End.Line, End.Character - 1);
        return Location.Create(path, new TextSpan(start.Line, length), new LinePositionSpan(start, end));
    }

    public void UpdateEnd(LinePosition end) =>
        End = NotInitializedEnd()
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

    public bool TryGetPropertyNode(string key, out JsonNode node) =>
        map.TryGetValue(key, out node);

    public IEnumerator<JsonNode> GetEnumerator() =>
        Kind == Kind.List ? list.GetEnumerator() : throw InvalidKind();

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();

    private InvalidOperationException InvalidKind() =>
        new($"Operation is not valid. Json kind is {Kind}");

    private bool NotInitializedEnd() =>
        End == LinePosition.Zero;
}
