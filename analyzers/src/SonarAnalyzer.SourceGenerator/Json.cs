/*
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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace SonarAnalyzer.SourceGenerator
{
    public static class Json
    {
        public static Dictionary<string, string> Parse(string json)
        {
            const string separator = @""": """;
            var ret = new Dictionary<string, string>();
            foreach (var line in json.Split('\n').Where(x => x.Contains(separator)))  // Ignoring CR LF
            {
                var index = line.IndexOf(separator);
                var key = line.Substring(0, index).TrimStart(' ', '"');
                var value = DecodeJsonString(line.Substring(index + separator.Length));
                ret.Add(key, value);
            }
            return ret;
        }

        // This is modified copy of SonarAnalyzer.Json.Parsing.LexicalAnalyzer.ReadStringValue()
        private static string DecodeJsonString(string fragment)
        {
            const int UnicodeEscapeLength = 4;
            var sb = new StringBuilder();
            var index = 0;
            while (fragment[index] != '"')
            {
                if (fragment[index] == '\\')
                {
                    index++;
                    switch (fragment[index])
                    {
                        case '"':
                            sb.Append('"');
                            break;
                        case '\\':
                            sb.Append('\\');
                            break;
                        case '/':
                            sb.Append('/');
                            break;
                        case 'b':
                            sb.Append('\b');
                            break;
                        case 'f':
                            sb.Append('\f');
                            break;
                        case 'n':
                            sb.Append('\n');
                            break;
                        case 'r':
                            sb.Append('\r');
                            break;
                        case 't':
                            sb.Append('\t');
                            break;
                        case 'u':
                            if (index + UnicodeEscapeLength >= fragment.Length)
                            {
                                throw new JsonException(@"Unexpected EOI, \uXXXX escape expected");
                            }
                            sb.Append(char.ConvertFromUtf32(int.Parse(fragment.Substring(index + 1, UnicodeEscapeLength), NumberStyles.HexNumber)));
                            index += UnicodeEscapeLength;
                            break;
                        default:
                            throw new JsonException($@"Unexpected escape sequence \{fragment[index]}");
                    }
                }
                else
                {
                    sb.Append(fragment[index]);
                }
                index++;
            }
            return sb.ToString();
        }
    }

    public sealed class JsonException : Exception
    {
        public JsonException(string message) : base(message) { }
    }
}
