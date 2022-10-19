using System;

namespace Tests.Diagnostics
{
    class Program
    {
        public const string Part1 = """test"""; // Noncompliant
        public const string Part2 = """test""";

        void Utf8StringLiterals()
        {
            ReadOnlySpan<byte> Part3 = "test"u8; // FN
            ReadOnlySpan<byte> Part4 = """test"""u8; // FN
        }
    }
}
