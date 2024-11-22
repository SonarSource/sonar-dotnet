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

using System;
using System.Globalization;
using System.Text;
using Microsoft.CodeAnalysis.Text;

namespace SonarAnalyzer.Json.Parsing
{
    internal class LexicalAnalyzer
    {
        private readonly List<string> lines = new();
        private int line;
        private int column = -1;

        public object Value { get; private set; }
        public LinePosition LastStart { get; private set; }
        private char CurrentChar => lines[line][column];
        private bool ReachedEndOfInput => line > lines.Count - 1 || (line == lines.Count - 1 && column >= lines[line].Length);

        public LexicalAnalyzer(string source)
        {
            var sb = new StringBuilder();
            foreach (var c in source.Replace("\r\n", "\n"))
            {
                if (c is '\n' or '\r' || char.GetUnicodeCategory(c) is UnicodeCategory.LineSeparator or UnicodeCategory.ParagraphSeparator)
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
            new(line, column + increment);

        public Symbol NextSymbol()
        {
            Value = null;
            NextPosition(false);
            SkipWhitespaceAndComments();
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
                case (>= '0' and <= '9') or '-':
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
                    throw new JsonException($"Unexpected character '{CurrentChar}'", LastStart);
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
                SeekToNextLine(throwIfReachedEndOfInput);
            }
        }

        private void SeekToNextLine(bool throwIfReachedEndOfInput)
        {
            do
            {
                line++;
            }
            while (line < lines.Count && lines[line].Length == 0);
            column = 0;
            if (throwIfReachedEndOfInput && ReachedEndOfInput)
            {
                throw new JsonException("Unexpected EOI", LastStart);
            }
        }

        private void SkipWhitespaceAndComments()
        {
            var isInMultiLineComment = false;
            while (!ReachedEndOfInput)
            {
                if (char.IsWhiteSpace(CurrentChar))
                {
                    NextPosition(false);
                }
                else if (IsSingleLineComment())
                {
                    // We just need to read starting from the next line
                    SeekToNextLine(false);
                }
                else if (IsEndOfMultiLineComment())
                {
                    isInMultiLineComment = false;
                    NextPosition(false); // Skip *
                    NextPosition(false); // Skip /
                }
                else if (IsMultiLineComment())
                {
                    if (!isInMultiLineComment)
                    {
                        // need to skip /* part of the comment
                        NextPosition(false); // Skip /
                        NextPosition(false); // Skip *
                    }
                    isInMultiLineComment = true;
                    NextPosition(); // we still want to fail on non-closed comments
                }
                else
                {
                    return;
                }
            }

            char? NextCharSameLine() =>
                (column + 1 < lines[line].Length) ? lines[line][column + 1] : (char?)null;

            bool IsSingleLineComment() =>
                CurrentChar == '/' && NextCharSameLine() == '/';

            bool IsMultiLineComment() =>
                isInMultiLineComment || (CurrentChar == '/' && NextCharSameLine() == '*');

            bool IsEndOfMultiLineComment() =>
                isInMultiLineComment && CurrentChar == '*' && NextCharSameLine() == '/';
        }

        private void ReadKeyword(string keyword)
        {
            for (var i = 0; i < keyword.Length; i++)
            {
                if (lines[line][column + i] != keyword[i])
                {
                    throw new JsonException($"Unexpected character '{lines[line][column + i]}'. Keyword '{keyword}' was expected", LastStart);
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
                                throw new JsonException(@"Unexpected EOI, \uXXXX escape expected", LastStart);
                            }
                            sb.Append(char.ConvertFromUtf32(int.Parse(lines[line].Substring(column + 1, UnicodeEscapeLength), NumberStyles.HexNumber)));
                            column += UnicodeEscapeLength;
                            break;
                        default:
                            throw new JsonException($@"Unexpected escape sequence \{CurrentChar}", LastStart);
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
            StringBuilder integral = new();
            StringBuilder current = integral;
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
                            throw new JsonException("Unexpected Number format: Unexpected '-'", LastStart);
                        }
                        break;
                    case (>= '0' and <= '9'):
                        current.Append(CurrentChar);
                        break;
                    case '.':
                        if (current == integral && current.ToString().TrimStart('-').Any())
                        {
                            @decimal = new StringBuilder();
                            current = @decimal;
                        }
                        else
                        {
                            throw new JsonException("Unexpected Number format: Unexpected '.'", LastStart);
                        }
                        break;
                    case '+':
                        if (current != exponent || current.Length != 0)
                        {
                            throw new JsonException("Unexpected Number format", LastStart);
                        }
                        break;
                    case 'e' or 'E':
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
                    ? (object)double.Parse(integral.ToString(), CultureInfo.InvariantCulture)
                    : decimal.Parse(integral + CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator + @decimal, CultureInfo.InvariantCulture);

                if (exponent == null)   // Integer or Decimal
                {
                    return baseValue;
                }
                else if (exponent.Length == 0 || exponent.ToString() == "-")
                {
                    throw new JsonException($"Unexpected Number exponent format: {exponent}", LastStart);
                }
                else
                {
                    return Convert.ToDouble(baseValue) * Math.Pow(10, int.Parse(exponent.ToString()));
                }
            }
        }
    }
}
