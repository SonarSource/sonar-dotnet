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

namespace SonarAnalyzer.Core.Common;

// ConsumedLength reports how many characters of the raw suffix were consumed to produce Candidate (which can differ from Candidate.Length, e.g.
// once quotes are stripped or escapes unescaped), so a caller scanning suffix for further, distinct occurrences of something can skip past it -
// text already established as belonging to this candidate must not be mistaken for a second, separate one.
public readonly record struct CandidateMatch(string Candidate, int ConsumedLength);

public static class SecretCandidateExtractor
{
    // A quoted value - in a connection string, a SQL literal, or most other contexts a leading quote can appear in - is delimited by its closing
    // quote regardless of what follows it. A value containing the separator is wrapped in matching quotes, with an embedded quote of the same
    // kind escaped by doubling it (e.g. Password="ab""cd";), so unwrap and unescape it before truncating at the separator, or a quoted secret
    // containing the separator would be cut short at that embedded occurrence instead of its real end.
    public static CandidateMatch ExtractCandidate(string suffix, char separator)
    {
        if (suffix.Length > 0 && suffix[0] is '"' or '\'')
        {
            var quote = suffix[0];
            var closingQuoteIndex = ClosingQuoteIndex(suffix, quote);
            if (closingQuoteIndex > 0)
            {
                var quotedCandidate = suffix.Substring(1, closingQuoteIndex - 1).Replace(new string(quote, 2), quote.ToString());
                return new CandidateMatch(quotedCandidate, closingQuoteIndex + 1);
            }
        }
        // No leading quote, or an unterminated one: fall back to truncating the raw text at the separator or a real line break - a credential
        // value spanning multiple lines is implausible, unlike one containing a plain space, so this doesn't risk cutting a legitimate secret
        // short the way splitting on any whitespace would - then drop a stray leading/trailing quote left over from it so it doesn't skew the
        // classifier's length/pattern checks against the equivalent unquoted value.
        var rawSegment = suffix.Split(separator, '\r', '\n')[0];
        var trimmedSegment = rawSegment.Trim();
        var candidate = trimmedSegment.Length > 0 && trimmedSegment[0] is '"' or '\''
            ? trimmedSegment.Substring(1).TrimEnd('"', '\'').Trim()
            : trimmedSegment.TrimEnd('"', '\'');
        return new CandidateMatch(candidate, rawSegment.Length);
    }

    // Best-effort only: recognizes the two most common escaping conventions for a quote inside a quoted value - doubling (e.g. "" for ADO.NET
    // connection strings) and a backslash prefix (e.g. \" for JSON/C-style strings) - but not both at once, and not any other convention. A
    // value that legitimately ends in a literal backslash right before its real closing quote is misread as escaped and falls back to plain
    // truncation instead; that's an accepted trade-off rather than a fully grammar-aware parser.
    private static int ClosingQuoteIndex(string value, char quote)
    {
        var i = 1;
        while (i < value.Length)
        {
            if (value[i] != quote)
            {
                i++;
            }
            else if (i + 1 < value.Length && value[i + 1] == quote)
            {
                i += 2; // Doubled-quote escape: skip the pair and keep looking for the real closing quote.
            }
            else if (value[i - 1] == '\\')
            {
                i++; // Backslash-prefixed escape: not a real close, keep looking. The backslash itself was already scanned past.
            }
            else
            {
                return i;
            }
        }
        return -1;
    }
}
