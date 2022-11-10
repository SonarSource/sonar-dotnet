using System;

namespace Tests.Diagnostics
{
    class Program
    {
        public const string RawStringLiteralsNonCompliant = """test"""; // Noncompliant
        public const string RawStringLiteralsCompliant = """test""";

        public const string InterpolatedStringNonCompliant = $"""test{RawStringLiteralsNonCompliant}"""; // Compliant, interpolated text is not considered
        public const string InterpolatedStringNonCompliant2 = $"""test{RawStringLiteralsCompliant}"""; // Noncompliant
        public const string InterpolatedStringCompliant = $"""test{RawStringLiteralsCompliant}""";

        void Utf8StringLiterals()
        {
            ReadOnlySpan<byte> Utf8Compliant = "test"u8;
            ReadOnlySpan<byte> Utf8Compliant2 = """test"""u8;
            ReadOnlySpan<byte> Utf8NonCompliant = "test"u8; // Noncompliant
            ReadOnlySpan<byte> Uft8NonCompliant2 = """test"""u8; // Noncompliant
        }

        void NewlinesInStringInterpolation()
        { 
            string NewlinesInterpolatedStringNonCompliant = $"test{RawStringLiteralsNonCompliant +
                RawStringLiteralsNonCompliant}"; // Compliant, interpolated text is not considered
            string NewlinesInterpolatedStringNonCompliant2 = $"test{RawStringLiteralsCompliant +
                RawStringLiteralsCompliant}"; // Noncompliant@-1
            string NewlinesInterpolatedStringCompliant = $"test{RawStringLiteralsCompliant +
                RawStringLiteralsCompliant}";

            string NewlinesInterpolatedStringRawNonCompliant = $$"""test{{RawStringLiteralsNonCompliant +
                RawStringLiteralsNonCompliant}}"""; // Compliant, interpolated text is not considered
            string NewlinesInterpolatedStringRawNonCompliant2 = $$"""test{{RawStringLiteralsCompliant +
                RawStringLiteralsCompliant}}"""; // Noncompliant@-1
            string NewlinesInterpolatedStringRawCompliant = $$"""test{{RawStringLiteralsCompliant +
                RawStringLiteralsCompliant}}""";
        }
    }
}
