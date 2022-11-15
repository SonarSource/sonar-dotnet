using System;

namespace Tests.Diagnostics
{
    class Program
    {
        public const string RawCompliant = """test"""; // Compliant
        public const string RawCompliantWithSpecialCharacter = """test"""; // Compliant, raw string

        public const string RawCompliantWithInterpolation = $"""test{RawCompliantWithSpecialCharacter}"""; // Compliant, raw string
        public const string RawCompliantWithInterpolationAndSpecialCharacter = $"""test{RawCompliant}"""; // Compliant, raw string

        void Utf8StringLiterals()
        {
            ReadOnlySpan<byte> Utf8Compliant = "test"u8; // Compliant
            ReadOnlySpan<byte> Utf8Noncompliant = "test"u8; // Noncompliant
            ReadOnlySpan<byte> Utf8VerbatimCompliant = @"test"u8; // Compliant, verbatim utf-8 string

            ReadOnlySpan<byte> Utf8CompliantRaw = """test"""u8; // Compliant, raw string
            ReadOnlySpan<byte> Utf8CompliantRawWithSpecialCharacter = """test"""u8; // Compliant, raw string

            ReadOnlySpan<byte> Utf8CompliantVerbatimRaw = @"""test"""u8; // Compliant, raw+verbatim string
            ReadOnlySpan<byte> Utf8CompliantVerbatimRawWithSpecialCharacter = @"""test"""u8; // Compliant, raw+verbatim string
        }

        void NewlinesInStringInterpolation()
        {
            string NewlinesInterpolatedStringNonCompliant = $"test{RawCompliantWithSpecialCharacter +
                RawCompliantWithSpecialCharacter}"; // Compliant, interpolated text is not considered
            string NewlinesInterpolatedStringNonCompliant2 = $"test{RawCompliant +
                RawCompliant}"; // Noncompliant@-1
            string NewlinesInterpolatedStringCompliant = $"test{RawCompliant +
                RawCompliant}";

            string NewlinesInterpolatedStringRawNonCompliant = $$"""test{{RawCompliantWithSpecialCharacter +
                RawCompliantWithSpecialCharacter}}"""; // Compliant, raw string
            string NewlinesInterpolatedStringRawNonCompliant2 = $$"""test{{RawCompliant +
                RawCompliant}}"""; // Compliant@-1, raw string
            string NewlinesInterpolatedStringRawCompliant = $$"""test{{RawCompliant +
                RawCompliant}}"""; // Compliant, raw string
        }
    }
}
