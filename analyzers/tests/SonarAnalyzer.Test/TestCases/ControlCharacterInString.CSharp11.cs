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
        }

        void NewlinesInStringInterpolation()
        {
            var compliant = "test"; // Compliant
            var nonCompliant = "test"; // Noncompliant

            var baseCase = $"test{
                compliant
                }"; // Compliant

            var interpolatedTextHasControlCharacter = $"test{
                nonCompliant
                }"; // Compliant, interpolated text ignored

            var normalTextHasControlCharacter = $"test{
                nonCompliant
                }"; // Noncompliant@-2

            var verbatimWithControlCharacter = @$"test{
                nonCompliant
                }"; // Compliant, verbatim

            var rawWithControlCharacter = $"""test{
                nonCompliant
                }"""; // Compliant, raw
        }
    }
}
