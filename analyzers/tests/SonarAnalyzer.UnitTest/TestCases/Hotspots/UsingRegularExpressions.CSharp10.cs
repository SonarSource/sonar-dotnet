using System;
using System.Text.RegularExpressions;

namespace Tests.Diagnostics
{
    class Program
    {
        const string part1 = "a";
        const string part2 = "b";
        const string compliant = $"{part1}{part2}";
        const string plus = "+";
        const string noncompliant = $"{part1}{plus}{part2}{plus}";

        void Main(string s)
        {
            Regex r;
            r = new Regex(compliant); // Compliant
            r = new Regex(noncompliant); // Noncompliant
        }
    }
}
