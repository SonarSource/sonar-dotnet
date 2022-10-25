using System;

namespace Tests.Diagnostics
{
    class Program
    {
        public const string RawStringLiteralsNonCompliant = """test"""; // Noncompliant
        public const string RawStringLiteralsCompliant = """test""";

        public const string InterpolatedStringNonCompliant = $"""test{RawStringLiteralsNonCompliant}"""; // FN
        public const string InterpolatedStringNonCompliant2 = $"""test{RawStringLiteralsCompliant}"""; // Noncompliant
        public const string InterpolatedStringCompliant = $"""test{RawStringLiteralsCompliant}""";

        void Utf8StringLiterals()
        {
            ReadOnlySpan<byte> Utf8Compliant = "test"u8;
            ReadOnlySpan<byte> Utf8Compliant2 = """test"""u8;
            ReadOnlySpan<byte> Utf8NonCompliant = "test"u8; // FN
            ReadOnlySpan<byte> Uft8NonCompliant2 = """test"""u8; // FN
        }
    }
}
