/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SonarAnalyzer.Core.Json.Test;

[TestClass]
public class JsonSerializerTest
{
    [TestMethod]
    public void Serialize_UnsupportedType() =>
        FluentActions.Invoking(() => JsonSerializer.Serialize(new { Value = new StringBuilder() })).Should().Throw<NotSupportedException>().WithMessage("Unexpected type: StringBuilder");

    [TestMethod]
    public void Serialize_BasicTypes()
    {
        var stringObjectMap = new Dictionary<string, object>
        {
            { "StringKey", "String value"},
            { "BoolKey", true},
            { "IntKey", 42}
        };
        var stringStringMap = ImmutableSortedDictionary<string, string>.Empty.Add("Key A", "Value A").Add("Key B", "Value B");
        var value = new TestData("Name", true, false, null, ["a", "b", "c"], StringComparison.CurrentCulture, stringObjectMap.ToArray(), stringStringMap.ToArray());

        var result = JsonSerializer.Serialize(value);
        result.ToUnixLineEndings().Should().Be("""
            {
              "stringValue": "Name",
              "trueValue": true,
              "falseValue": false,
              "nullString": null,
              "stringArray": ["a", "b", "c"],
              "enumValue": "CurrentCulture",
              "stringObjectMap": [
                { "key": "StringKey", "value": "String value" },
                { "key": "BoolKey", "value": true },
                { "key": "IntKey", "value": 42 }
              ],
              "stringStringMap": [
                { "key": "Key A", "value": "Value A" },
                { "key": "Key B", "value": "Value B" }
              ]
            }
            """);
        // And it also deserializes correctly
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, Converters = { new JsonStringEnumConverter(), new PrimitiveObjectConverter() } };
        System.Text.Json.JsonSerializer.Deserialize<TestData>(result, options).Should().BeEquivalentTo(value);
    }

    [TestMethod]
    public void Serialize_Encoding() =>
        JsonSerializer.Serialize(new { Value = "Start \\ \" \n \r \t \b \f End" }).ToUnixLineEndings().Should().Be("""
            {
              "value": "Start \\ \" \n \r \t \b \f End"
            }
            """);

    [TestMethod]
    public void Serialize_ObjectWithIndexer() =>
        JsonSerializer.Serialize(new List<int> { 42, 43 }).ToUnixLineEndings().Should().Be("""
            {
              "capacity": 4,
              "count": 2
            }
            """);

    private sealed record TestData(string StringValue,
                                   bool TrueValue,
                                   bool FalseValue,
                                   string NullString,
                                   string[] StringArray,
                                   StringComparison EnumValue,
                                   KeyValuePair<string, object>[] StringObjectMap,
                                   KeyValuePair<string, string>[] StringStringMap)
    {
        public TestData() : this(null, true, false, null, null, StringComparison.Ordinal, null, null) { }
    }

    private sealed class PrimitiveObjectConverter : JsonConverter<object>
    {
        // This makes KeyValuePair<string, object> to deserialize to actual value. The default behavior is that object holds general JsonElement instead.
        public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
            reader.TokenType switch
            {
                JsonTokenType.True => true,
                JsonTokenType.False => false,
                JsonTokenType.Null => null,
                JsonTokenType.String => reader.GetString(),
                JsonTokenType.Number => reader.GetDouble(),
                _ => throw new InvalidOperationException("Unexpected token type: " + reader.TokenType)
            };

        public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options) =>
            throw new NotSupportedException();
    }
}
