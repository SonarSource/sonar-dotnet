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

using System.Globalization;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Json;
using SonarAnalyzer.Json.Parsing;

namespace SonarAnalyzer.UnitTest.Common
{
    [TestClass]
    public class LexicalAnalyzerTest
    {
        [TestMethod]
        public void IgnoresWhiteSpace()
        {
            var sut = new LexicalAnalyzer("   \t\n\r [ \n \r ] \r\n");
            sut.NextSymbol().Should().Be(Symbol.OpenSquareBracket);
            sut.NextSymbol().Should().Be(Symbol.CloseSquareBracket);
            sut.NextSymbol().Should().Be(Symbol.EndOfInput);
        }

        [TestMethod]
        public void ReadSpecialCharacters()
        {
            var sut = new LexicalAnalyzer("{{[[,,]]}}::");
            sut.NextSymbol().Should().Be(Symbol.OpenCurlyBracket);
            sut.NextSymbol().Should().Be(Symbol.OpenCurlyBracket);
            sut.NextSymbol().Should().Be(Symbol.OpenSquareBracket);
            sut.NextSymbol().Should().Be(Symbol.OpenSquareBracket);
            sut.NextSymbol().Should().Be(Symbol.Comma);
            sut.NextSymbol().Should().Be(Symbol.Comma);
            sut.NextSymbol().Should().Be(Symbol.CloseSquareBracket);
            sut.NextSymbol().Should().Be(Symbol.CloseSquareBracket);
            sut.NextSymbol().Should().Be(Symbol.CloseCurlyBracket);
            sut.NextSymbol().Should().Be(Symbol.CloseCurlyBracket);
            sut.NextSymbol().Should().Be(Symbol.Colon);
            sut.NextSymbol().Should().Be(Symbol.Colon);
            sut.NextSymbol().Should().Be(Symbol.EndOfInput);
        }

        [DataTestMethod]
        [DataRow("0")]
        [DataRow("000")]
        [DataRow("-0")]
        [DataRow("-1")]
        [DataRow("-42")]
        [DataRow("-42424242")]
        [DataRow("1")]
        [DataRow("2")]
        [DataRow("3")]
        [DataRow("4")]
        [DataRow("5")]
        [DataRow("6")]
        [DataRow("7")]
        [DataRow("8")]
        [DataRow("42")]
        [DataRow("42424242")]
        [DataRow("1234567890")]
        public void ReadNumber_Integer(string source)
        {
            var sut = new LexicalAnalyzer(source);
            sut.NextSymbol().Should().Be(Symbol.Value);
            sut.Value.Should().BeOfType<int>().And.Be(int.Parse(source));
            sut.NextSymbol().Should().Be(Symbol.EndOfInput);
        }

        [DataTestMethod]
        [DataRow("0.0")]
        [DataRow("000.000")]
        [DataRow("111.111")]
        [DataRow("424242.5555555")]
        public void ReadNumber_Decimal(string source)
        {
            var expected = decimal.Parse(source, CultureInfo.InvariantCulture);
            var sut = new LexicalAnalyzer(source);
            sut.NextSymbol().Should().Be(Symbol.Value);
            sut.Value.Should().BeOfType<decimal>().And.Be(expected);
            sut.NextSymbol().Should().Be(Symbol.EndOfInput);
        }

        [DataTestMethod]
        [DataRow("0e0", 0.0)]
        [DataRow("1e1", 10.0)]
        [DataRow("42e0", 42.0)]
        [DataRow("42e-1", 4.2)]
        [DataRow("42E-1", 4.2)]
        [DataRow("42e1", 420.0)]
        [DataRow("42e+1", 420.0)]
        [DataRow("42E+1", 420.0)]
        [DataRow("8e8", 800_000_000)]
        [DataRow("-42e1", -420.0)]
        [DataRow("-42e-1", -4.2)]
        [DataRow("-42e+1", -420.0)]
        [DataRow("4.2e1", 42.0)]
        [DataRow("44.22e2", 4422.0)]
        public void ReadNumber_Double(string source, double expected)
        {
            var sut = new LexicalAnalyzer(source);
            sut.NextSymbol().Should().Be(Symbol.Value);
            sut.Value.Should().BeOfType<double>().And.Be(expected);
            sut.NextSymbol().Should().Be(Symbol.EndOfInput);
        }

        [DataTestMethod]
        [DataRow(" \"\" ", "")]
        [DataRow(" \"Lorem Ipsum\" ", "Lorem Ipsum")]
        [DataRow(" \"Quote\\\"Quote\" ", "Quote\"Quote")]
        [DataRow(" \"Slash\\/ Backslash\\\\\" ", "Slash/ Backslash\\")]
        [DataRow(" \"Special B\\b F\\f N\\n R\\r T\\t\" ", "Special B\b F\f N\n R\r T\t")]
        [DataRow(" \"Unicode\u0158\u0159\" ", "UnicodeŘř")]
        [DataRow(@"""\u0159""", "ř")]
        public void ReadString(string source, string expected)
        {
            var sut = new LexicalAnalyzer(source);
            sut.NextSymbol().Should().Be(Symbol.Value);
            sut.Value.Should().BeOfType<string>().And.Be(expected);
            sut.NextSymbol().Should().Be(Symbol.EndOfInput);
        }

        [DataTestMethod]
        [DataRow("null", null)]
        [DataRow("true", true)]
        [DataRow("false", false)]
        public void ReadKeyword(string source, object expected)
        {
            var sut = new LexicalAnalyzer(source);
            sut.NextSymbol().Should().Be(Symbol.Value);
            sut.Value.Should().Be(expected);
            sut.NextSymbol().Should().Be(Symbol.EndOfInput);
        }

        [DataTestMethod]
        [DataRow(".", "Unexpected character '.' at line 1 position 1")]
        [DataRow("tx", "Unexpected character 'x'. Keyword 'true' was expected at line 1 position 1")]
        [DataRow(@"""\u", @"Unexpected EOI, \uXXXX escape expected at line 1 position 1")]
        [DataRow(@"""\u12", @"Unexpected EOI, \uXXXX escape expected at line 1 position 1")]
        [DataRow(@"""\u12345", @"Unexpected EOI at line 1 position 1")]
        [DataRow(@"""\x", @"Unexpected escape sequence \x at line 1 position 1")]
        [DataRow(@"""\", @"Unexpected EOI at line 1 position 1")]
        [DataRow("0-", "Unexpected number format: Unexpected '-' at line 1 position 1")]
        [DataRow("-.", "Unexpected number format: Unexpected '.' at line 1 position 1")]
        [DataRow("0..", "Unexpected number format: Unexpected '.' at line 1 position 1")]
        [DataRow("0.0.", "Unexpected number format: Unexpected '.' at line 1 position 1")]
        [DataRow("0e0.0", "Unexpected number format: Unexpected '.' at line 1 position 1")]
        [DataRow("0+", "Unexpected number format at line 1 position 1")]
        [DataRow("0.0+", "Unexpected number format at line 1 position 1")]
        [DataRow("0e0+0", "Unexpected number format at line 1 position 1")]
        [DataRow("0e", "Unexpected number exponent format:  at line 1 position 1")]
        [DataRow("0e-", "Unexpected number exponent format: - at line 1 position 1")]
        public void InvalidInput_ThrowsJsonException(string source, string expectedMessage)
        {
            var sut = new LexicalAnalyzer(source);
            sut.Invoking(x => x.NextSymbol()).Should().Throw<JsonException>().WithMessage(expectedMessage);
        }
    }
}
