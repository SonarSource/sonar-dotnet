using System;
using System.IO;

namespace Tests.Diagnostics
{
    public class Program
    {
        public void Examples()
        {
            const string t = "T";
            const string e = "E";
            const string m = "M";
            const string p = "P";
            const string part1 = "/tEmP"; // Noncompliant
            const string part2 = "/f";
            const string noncompliant2 = $"{part1}{part2}"; // Noncompliant

            var tmp = Environment.GetEnvironmentVariable($"{t}{e}{m}{p}"); // Noncompliant
            tmp = Environment.GetEnvironmentVariable($"{t}{e}{m}{p}{5}");
        }
    }
}
