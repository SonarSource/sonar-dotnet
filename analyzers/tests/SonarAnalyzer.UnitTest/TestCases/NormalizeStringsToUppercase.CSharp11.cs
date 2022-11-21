using System.Globalization;

namespace Tests.Diagnostics
{
    class Program
    {
        void StringLowerCalls()
        {
            var score = 100;
            var s1 = $"The score: {score} is {score switch
            {
                > 0 => "Positive",
                < 0 => "Negative",
                _ => "Zero",
            }}".ToLower(); // Compliant

            var s2 = $"The score: {score} is {score switch
            {
                > 0 => "Positive",
                < 0 => "Negative",
                _ => "Zero",
            }}".ToLowerInvariant(); // Noncompliant
        }
    }
}
