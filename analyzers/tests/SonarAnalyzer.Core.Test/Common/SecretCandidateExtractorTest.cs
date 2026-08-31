/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.Core.Common.Test;

[TestClass]
public class SecretCandidateExtractorTest
{
    [TestMethod]
    [DataRow("hardcoded", "hardcoded", 9)]
    [DataRow("", "", 0)]
    [DataRow("ab12", "ab12", 4)]
    // Quoted value containing the separator: the whole quoted content is the candidate, not just up to the embedded separator.
    [DataRow("\"ab;cdefghijk\"", "ab;cdefghijk", 14)]
    [DataRow("'ab;cdefghijk'", "ab;cdefghijk", 14)]
    // Doubled quote inside the value is an escape, not the closing quote.
    [DataRow("\"ab\"\"cd;ef\"", "ab\"cd;ef", 11)]
    // Unterminated quote: falls back to truncating at the separator, then strips the stray leading quote.
    [DataRow("\"hardcodedlongvalue;more", "hardcodedlongvalue", 19)]
    // The closing quote is trusted regardless of what follows it - whitespace and more text included, none of which is part of ConsumedLength.
    [DataRow("\"ab;cdefghijk\" ;Next=1", "ab;cdefghijk", 14)]
    [DataRow("'vwxyz' extra text", "vwxyz", 7)]
    [DataRow("'ab;cdefghijk' WHERE id=1;", "ab;cdefghijk", 14)]
    // A stray trailing quote with no leading one is stripped from the fallback candidate too.
    [DataRow("hardcoded\"", "hardcoded", 10)]
    // Mismatched quote type inside the value: the opening quote never finds a matching close of the SAME type, so it falls back to plain truncation.
    [DataRow("'S3crEt\"123", "S3crEt\"123", 11)]
    // A different quote character embedded in a properly-closed value doesn't confuse the parser - only the opening quote's own type is looked for.
    [DataRow("\"S3crEt;'123\"", "S3crEt;'123", 13)]
    // An unquoted value spanning a real line break (e.g. a multi-line C# verbatim string literal) is truncated at the line break too, not just ';'.
    [DataRow("hardcoded\nsome other text", "hardcoded", 9)]
    [DataRow("hardcoded\r\nsome other text", "hardcoded", 9)]
    // Backslash-prefixed escape (JSON/C-style, e.g. \"): recognized as an escaped quote, not the closing delimiter, alongside doubled quotes.
    [DataRow("\"ab\\\"cd;efgh\"", "ab\\\"cd;efgh", 13)]
    public void ExtractCandidate_ReturnsExpectedCandidateAndConsumedLength(string suffix, string expectedCandidate, int expectedConsumedLength)
    {
        var (candidate, consumedLength) = SecretCandidateExtractor.ExtractCandidate(suffix, ';');
        candidate.Should().Be(expectedCandidate);
        consumedLength.Should().Be(expectedConsumedLength);
    }

    // Accepted, documented limitation: only one escaping convention is recognized at a time, so a value that legitimately ends in a literal
    // backslash right before its real closing quote is misread as an escaped quote. It still degrades gracefully to plain truncation rather
    // than behaving unpredictably - this test pins that fallback behavior, not a claim that the boundary is found correctly.
    [TestMethod]
    public void ExtractCandidate_ValueEndingInBackslash_FallsBackGracefully()
    {
        var (candidate, consumedLength) = SecretCandidateExtractor.ExtractCandidate("\"C:\\Temp\\\"", ';');
        candidate.Should().Be("C:\\Temp\\");
        consumedLength.Should().Be(10);
    }

    [TestMethod]
    public void ExtractCandidate_ConsumedLength_NeverExceedsSuffixLength()
    {
        foreach (var suffix in new[] { "\"a;b\"", "'a;b'", "a;b", "\"unterminated;more", "'a' trailing text", string.Empty })
        {
            var (_, consumedLength) = SecretCandidateExtractor.ExtractCandidate(suffix, ';');
            consumedLength.Should().BeLessThanOrEqualTo(suffix.Length);
        }
    }
}
