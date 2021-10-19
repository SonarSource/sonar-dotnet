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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis.Text;

namespace SonarAnalyzer.Json.Parsing
{
    internal class LexicalAnalyzer
    {
        private readonly List<string> lines = new List<string>();
        private int line;
        private int column = -1;

        public object Value { get; private set; }
        public LinePosition LastStart { get; private set; }
        private char CurrentChar => lines[line][column];
        private bool ReachedEndOfInput => line > lines.Count - 1 || (line == lines.Count - 1 && column >= lines[line].Length);
        private string AtLocationSuffix => $" at line {LastStart.Line + 1} position {LastStart.Character + 1}";

        public LexicalAnalyzer(string source)
        {
            var sb = new StringBuilder();
            foreach (var c in source.Replace("\r\n", "\n"))
            {
                if (c == '\n'
                    || c == '\r'
                    || (char.GetUnicodeCategory(c) is var category && (category == UnicodeCategory.LineSeparator || category == UnicodeCategory.ParagraphSeparator)))
                {
                    lines.Add(sb.ToString());
                    sb.Clear();
                }
                else
                {
                    sb.Append(c);
                }
            }
            lines.Add(sb.ToString());
        }

        public LinePosition CurrentPosition(int increment) =>
            new LinePosition(line, column + increment);

        public Symbol NextSymbol()
        {
            Value = null;
            NextPosition(false);
            SkipWhiteSpace();               // FIXME: This should be SkipWhitespaceAndComments(), current implementation will throw on comments
            if (ReachedEndOfInput)
            {
                return Symbol.EndOfInput;
            }
            LastStart = CurrentPosition(0);
            switch (CurrentChar)
            {
                case '{':
                    return Symbol.OpenCurlyBracket;
                case '}':
                    return Symbol.CloseCurlyBracket;
                case '[':
                    return Symbol.OpenSquareBracket;
                case ']':
                    return Symbol.CloseSquareBracket;
                case ',':
                    return Symbol.Comma;
                case ':':
                    return Symbol.Colon;
                case '"':
                    Value = ReadStringValue();
                    return Symbol.Value;
                case '-':
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    Value = ReadNumberValue();
                    return Symbol.Value;
                case 'n':
                    ReadKeyword("null");
                    Value = null;
                    return Symbol.Value;
                case 't':
                    ReadKeyword("true");
                    Value = true;
                    return Symbol.Value;
                case 'f':
                    ReadKeyword("false");
                    Value = false;
                    return Symbol.Value;
                default:
                    throw new JsonException($"Unexpected character '{CurrentChar}'{AtLocationSuffix}");
            }
        }

        private void NextPosition(bool throwIfReachedEndOfInput = true)
        {
            if (line < lines.Count && column < lines[line].Length - 1)
            {
                column++;
            }
            else
            {
                do
                {
                    line++;
                }
                while (line < lines.Count && lines[line].Length == 0);
                column = 0;
                if (throwIfReachedEndOfInput && ReachedEndOfInput)
                {
                    throw new JsonException($"Unexpected EOI{AtLocationSuffix}");
                }
            }
        }

        private void SkipWhiteSpace()
        {
            while (!ReachedEndOfInput && char.IsWhiteSpace(CurrentChar))
            {
                NextPosition(false);
            }
        }

        private void ReadKeyword(string keyword)
        {
            for (var i = 0; i < keyword.Length; i++)
            {
                if (lines[line][column + i] != keyword[i])
                {
                    throw new JsonException($"Unexpected character '{lines[line][column + i]}'{AtLocationSuffix}. Keyword '{keyword}' was expected.");
                }
            }
            column += keyword.Length - 1;
        }

        private string ReadStringValue()
        {
            const int UnicodeEscapeLength = 4;
            var sb = new StringBuilder();
            NextPosition();  // Skip quote
            while (CurrentChar != '"')
            {
                if (CurrentChar == '\\')
                {
                    NextPosition();
                    switch (CurrentChar)
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
                            if (column + UnicodeEscapeLength >= lines[line].Length)
                            {
                                throw new JsonException($@"Unexpected EOI, \uXXXX escape expected{AtLocationSuffix}");
                            }
                            sb.Append(char.ConvertFromUtf32(int.Parse(lines[line].Substring(column + 1, UnicodeEscapeLength), NumberStyles.HexNumber)));
                            column += UnicodeEscapeLength;
                            break;
                        default:
                            throw new JsonException($@"Unexpected escape sequence \{CurrentChar}{AtLocationSuffix}");
                    }
                }
                else
                {
                    sb.Append(CurrentChar);
                }
                NextPosition();
            }
            return sb.ToString();
        }

        private object ReadNumberValue()
        {
            StringBuilder @decimal = null;
            StringBuilder exponent = null;
            var integer = new StringBuilder();
            var current = integer;
            while (!ReachedEndOfInput)
            {
                switch (CurrentChar)
                {
                    case '-':
                        if (current.Length == 0)
                        {
                            current.Append('-');
                        }
                        else
                        {
                            throw new JsonException($"Unexpected number format: Unexpected '-'{AtLocationSuffix}");
                        }
                        break;
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        current.Append(CurrentChar);
                        break;
                    case '.':
                        if (current == integer && current.ToString().TrimStart('-').Any())
                        {
                            @decimal = new StringBuilder();
                            current = @decimal;
                        }
                        else
                        {
                            throw new JsonException($"Unexpected number format: Unexpected '.'{AtLocationSuffix}");
                        }
                        break;
                    case '+':
                        if (current != exponent || current.Length != 0)
                        {
                            throw new JsonException($"Unexpected number format{AtLocationSuffix}");
                        }
                        break;
                    case 'e':
                    case 'E':
                        exponent = new StringBuilder();
                        current = exponent;
                        break;
                    default:
                        // Remain on the last digit, position cannot be zero here, since we at least read one digit character
                        column--;
                        return BuildResult();
                }
                NextPosition(false);
            }
            return BuildResult();

            object BuildResult()
            {
                var baseValue = @decimal == null
                    ? (object)int.Parse(integer.ToString())
                    : decimal.Parse(integer + CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator + @decimal);
                if (exponent == null)   // Integer or Decimal
                {
                    return baseValue;
                }
                else if (exponent.Length == 0 || exponent.ToString() == "-")
                {
                    throw new JsonException($"Unexpected number exponent format: {exponent}{AtLocationSuffix}");
                }
                else
                {
                    return Convert.ToDouble(baseValue) * Math.Pow(10, int.Parse(exponent.ToString()));
                }
            }
        }
    }
}
