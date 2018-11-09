using System;
using System.Text.RegularExpressions;
using RE = System.Text.RegularExpressions.Regex;
using static System.Text.RegularExpressions.Regex;

namespace Tests.Diagnostics
{
    class Program
    {
        void Main()
        {
            Regex r;
            r = new Regex(""); // Noncompliant {{Make sure that using a regular expression is safe here.}}
//              ^^^^^^^^^^^^^
            r = new Regex("", RegexOptions.Compiled); // Noncompliant
            r = new Regex("", RegexOptions.Compiled, TimeSpan.Zero); // Noncompliant

            Regex.IsMatch("", ""); // Noncompliant
//          ^^^^^^^^^^^^^^^^^^^^^
            Regex.IsMatch("", "", RegexOptions.Compiled); // Noncompliant
            Regex.IsMatch("", "", RegexOptions.Compiled, TimeSpan.Zero); // Noncompliant

            Regex.Match("", ""); // Noncompliant
            Regex.Match("", "", RegexOptions.Compiled); // Noncompliant
            Regex.Match("", "", RegexOptions.Compiled, TimeSpan.Zero); // Noncompliant

            Regex.Matches("", ""); // Noncompliant
            Regex.Matches("", "", RegexOptions.Compiled); // Noncompliant
            Regex.Matches("", "", RegexOptions.Compiled, TimeSpan.Zero); // Noncompliant

            Regex.Replace("", "", match => ""); // Noncompliant
            Regex.Replace("", "", ""); // Noncompliant
            Regex.Replace("", "", match => "", RegexOptions.Compiled); // Noncompliant
            Regex.Replace("", "", "", RegexOptions.Compiled); // Noncompliant
            Regex.Replace("", "", match => "", RegexOptions.Compiled, TimeSpan.Zero); // Noncompliant
            Regex.Replace("", "", "", RegexOptions.Compiled, TimeSpan.Zero); // Noncompliant

            Regex.Split("", ""); // Noncompliant
            Regex.Split("", "", RegexOptions.Compiled); // Noncompliant
            Regex.Split("", "", RegexOptions.Compiled, TimeSpan.Zero); // Noncompliant

            new System.Text.RegularExpressions.Regex(""); // Noncompliant
            new RE(""); // Noncompliant
            System.Text.RegularExpressions.Regex.IsMatch("", ""); // Noncompliant
            RE.IsMatch("", ""); // Noncompliant
            IsMatch("", ""); // Noncompliant

            // Non-static methods are compliant
            r.IsMatch("");
            r.IsMatch("", 0);

            r.Match("");
            r.Match("", 0);
            r.Match("", 0, 1);

            r.Matches("");
            r.Matches("", 0);

            r.Replace("", match => "");
            r.Replace("", "");
            r.Replace("", match => "", 0);
            r.Replace("", "", 0);
            r.Replace("", match => "", 0, 0);
            r.Replace("", "", 0, 0);

            r.Split("");
            r.Split("", 0);
            r.Split("", 0, 0);
        }
    }
}
