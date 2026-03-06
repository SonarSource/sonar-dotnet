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

namespace SonarAnalyzer.Core.Json;

public static class JsonSerializer
{
    public static string Serialize(object value) =>
        SerializeLines("{", "}", value.GetType().GetProperties().Where(x => x.GetIndexParameters().Length == 0).Select(x => $"""  "{SerializeKey(x.Name)}": {SerializeValue(x.GetValue(value))}"""));

    private static string SerializeKey(string key) =>
        char.ToLowerInvariant(key[0]) + key.Substring(1);

    private static string SerializeValue(object original) =>
        original switch
        {
            null => "null",
            bool value => value.ToString().ToLower(),
            int value => value.ToString(),
            string value => SerializeValue(value),
            Enum => SerializeValue(original.ToString()),
            IEnumerable<string> values => $"[{values.JoinStr(", ", SerializeValue)}]",
            IEnumerable<KeyValuePair<string, object>> values => SerializePairs(values, SerializeValue),
            IEnumerable<KeyValuePair<string, string>> values => SerializePairs(values, SerializeValue),
            _ => throw new NotSupportedException($"Unexpected type: {original.GetType().Name}")
        };

    private static string SerializeValue(string value)
    {
        var sb = new StringBuilder();
        sb.Append(@"""");
        foreach (var ch in value)
        {
            sb.Append(ch switch
            {
                '\\' => @"\\",
                '\"' => @"\""",
                '\n' => @"\n",
                '\r' => @"\r",
                '\t' => @"\t",
                '\b' => @"\b",
                '\f' => @"\f",
                _ => ch
            });
        }
        sb.Append(@"""");
        return sb.ToString();
    }

    private static string SerializePairs<TValue>(IEnumerable<KeyValuePair<string, TValue>> pairs, Func<TValue, string> serializeValue) =>
        SerializeLines("[", "  ]", pairs.Select(x => $$"""    { "key": {{SerializeValue(x.Key)}}, "value": {{serializeValue(x.Value)}} }"""));

    private static string SerializeLines(string prefix, string suffix, IEnumerable<string> lines)
    {
        var sb = new StringBuilder();
        sb.AppendLine(prefix);
        sb.AppendLine(lines.JoinStr($",{Environment.NewLine}"));
        sb.Append(suffix);
        return sb.ToString();
    }
}
