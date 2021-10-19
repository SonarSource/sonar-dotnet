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

using Microsoft.CodeAnalysis.Text;

namespace SonarAnalyzer.Json.Parsing
{
    internal class SyntaxAnalyzer
    {
        // Parse   -> { ParseObject | [ ParseList

        // ParseObject -> } | ObjectKeyValue ObjectRest
        // ObjectRest -> , ObjectKeyValue ObjectRest | }
        // ObjectKeyValue -> String : ParseValue

        // ParseList -> ] | ParseValue ArrayRest
        // ArrayRest -> , ParseValue ] | ]

        // ParseValue -> { ParseObject | [ ParseList | Symbol.Value (Where .Value is true | false | null | String | Number)

        private readonly LexicalAnalyzer lexer;
        private Symbol symbol;

        public SyntaxAnalyzer(string source) =>
            lexer = new LexicalAnalyzer(source);

        public JsonNode Parse() =>
            ReadNext() switch
            {
                Symbol.OpenCurlyBracket => ParseObject(),
                Symbol.OpenSquareBracket => ParseList(),
                _ => throw Unexpected("{ or [")
            };

        private Symbol ReadNext() =>
            symbol = lexer.NextSymbol();

        private JsonNode ParseObject()
        {
            var ret = new JsonNode(lexer.LastStart, Kind.Object);
            if (ReadNext() != Symbol.CloseCurlyBracket)  // Could be empty object {}
            {
                ObjectKeyValue(ret);
                while (ReadNext() == Symbol.Comma)
                {
                    ReadNext();
                    ObjectKeyValue(ret);
                }
            }
            ret.UpdateEnd(new LinePosition(lexer.LastStart.Line, lexer.LastStart.Character + 1)); // FIXME: Verify that Roslyn needs to be one character behind the end, otherwise, we may use LastStart
            return ret;
        }

        private void ObjectKeyValue(JsonNode target)
        {
            if (symbol == Symbol.Value && lexer.Value is string key)
            {
                if (ReadNext() != Symbol.Colon)
                {
                    throw Unexpected(":");
                }
                ReadNext(); // Prepare before reading Value
                target.Add(key, ParseValue());
            }
            else
            {
                throw Unexpected("String Value");
            }
        }

        private JsonNode ParseList()
        {
            var ret = new JsonNode(lexer.LastStart, Kind.List);
            if (ReadNext() != Symbol.CloseSquareBracket)    // Could be empty array []
            {
                ret.Add(ParseValue());
                while (ReadNext() == Symbol.Comma)
                {
                    ReadNext();
                    ret.Add(ParseValue());
                }
                if (symbol != Symbol.CloseSquareBracket)
                {
                    throw Unexpected("]");
                }
            }
            ret.UpdateEnd(new LinePosition(lexer.LastStart.Line, lexer.LastStart.Character + 1)); // FIXME: Verify that Roslyn needs to be one character behind the end, otherwise, we may use LastStart
            return ret;
        }

        private JsonNode ParseValue() =>
            // Symbol is already read
            symbol switch
            {
                Symbol.OpenCurlyBracket => ParseObject(),
                Symbol.OpenSquareBracket => ParseList(),
                Symbol.Value => new JsonNode(lexer.LastStart, lexer.CurrentPosition(1), lexer.Value),
                _ => throw Unexpected("{, [ or Value (true, false, null, String, Number)")
            };

        private JsonException Unexpected(string expected) =>
            new JsonException($"{expected} expected, but {symbol} found.");
    }
}
