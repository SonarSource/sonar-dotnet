using System;

namespace Tests.TestCases
{
    class UseStringIsNullOrEmpty
    {
        public const string ConstEmptyString = """

                                               """;

        public const string InterPolatedString = $$"""{{ConstEmptyString}}""";

        public void Test(string value)
        {
            if (value.Equals(InterPolatedString)) // Noncompliant {{Use 'string.IsNullOrEmpty()' instead of comparing to empty string.}}
            { }

            // Noncompliant@+1
            if (value.Equals("""
                    
                             """))

            { }
        }

        public void SpanMatch(Span<char> span, ReadOnlySpan<char> readonlySpan)
        {
            var a = span is """

                            """; // Compliant

            var b = readonlySpan is """

                                    """; // Compliant
        }

        public bool ListPattern(string[] uris) =>
            uris is ["""

                     """,
                     """

                     """]; // Compliant
    }
}
