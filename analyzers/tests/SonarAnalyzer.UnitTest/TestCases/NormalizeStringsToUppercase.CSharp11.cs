using System.Globalization;

namespace Tests.Diagnostics
{
    class Program
    {
        void StringLowerCalls()
        {
            var score = 100;
            var newLineInInterpolation = $"The score: {score} is {score switch
            {
                > 0 => "Positive",
                < 0 => "Negative",
                _ => "Zero",
            }}".ToLower(); // Compliant

            var newLineInInterpolation2 = $"The score: {score} is {score switch
            {
                > 0 => "Positive",
                < 0 => "Negative",
                _ => "Zero",
            }}".ToLowerInvariant(); // Noncompliant

            var rawStringLiteral = """
                This is a long message.
                It has several lines.
                    Some are indented.
                """.ToLower(); // Compliant

            var rawStringLiteral2 = """
                This is a long message.
                It has several lines.
                    Some are indented.
                """.ToLowerInvariant(); // Noncompliant
        }
    }
}
