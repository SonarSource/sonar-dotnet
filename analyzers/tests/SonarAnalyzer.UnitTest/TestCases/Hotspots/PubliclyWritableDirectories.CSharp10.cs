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
            const string compliant = $"{p}{m}{e}{t}";
            const string noncompliant = $"{t}{e}{m}{p}";

            var tmp1 = Environment.GetEnvironmentVariable(compliant);
            var tmp2 = Environment.GetEnvironmentVariable(noncompliant); // FN
        }
    }
}
