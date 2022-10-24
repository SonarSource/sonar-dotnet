using System;
using System.Text.RegularExpressions;

namespace Tests.Diagnostics
{
    class Program
    {
        void Main(string s)
        {
            Regex r;
            r = new Regex("""{abc}+{a}"""); // Noncompliant
        }
    }
}
