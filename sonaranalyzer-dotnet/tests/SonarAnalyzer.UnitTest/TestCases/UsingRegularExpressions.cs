using System;
using System.Text.RegularExpressions;
using RE = System.Text.RegularExpressions.Regex;
using static System.Text.RegularExpressions.Regex;

namespace Tests.Diagnostics
{
    class Program
    {
        void Main(string s)
        {
            Regex r;
            r = new Regex(""); // Compliant, less than 3 characters
            r = new Regex("**"); // Compliant, less than 3 characters
            r = new Regex("+*"); // Compliant, less than 3 characters
            r = new Regex("abcdefghijklmnopqrst"); // Compliant, does not have the special characters
            r = new Regex("abcdefghijklmnopqrst+"); // Compliant, has only 1 special character
            r = new Regex("{abc}+defghijklmnopqrst"); // Noncompliant
            r = new Regex("{abc}+{a}"); // Noncompliant {{Make sure that using a regular expression is safe here.}}
//              ^^^^^^^^^^^^^^^^^^^^^^
            r = new Regex("+++"); // Noncompliant
            r = new Regex(@"\+\+\+"); // Noncompliant FP (escaped special characters)
            r = new Regex("{{{"); // Noncompliant
            r = new Regex(@"\{\{\{"); // Noncompliant FP (escaped special characters)
            r = new Regex("***"); // Noncompliant
            r = new Regex(@"\*\*\*"); // Noncompliant FP (escaped special characters)
            r = new Regex("(a+)+s", RegexOptions.Compiled); // Noncompliant
            r = new Regex("(a+)+s", RegexOptions.Compiled, TimeSpan.Zero); // Noncompliant
            r = new Regex("{ab}*{ab}+{cd}+foo*"); // Noncompliant

            Regex.IsMatch("", "(a+)+s"); // Noncompliant
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^
            Regex.IsMatch(s, "(a+)+s", RegexOptions.Compiled); // Noncompliant
            Regex.IsMatch("", "{foo}{bar}", RegexOptions.Compiled, TimeSpan.Zero); // Noncompliant

            Regex.Match(s, "{foo}{bar}"); // Noncompliant
            Regex.Match("", "{foo}{bar}", RegexOptions.Compiled); // Noncompliant
            Regex.Match("", "{foo}{bar}", RegexOptions.Compiled, TimeSpan.Zero); // Noncompliant

            Regex.Matches(s, "{foo}{bar}"); // Noncompliant
            Regex.Matches("", "{foo}{bar}", RegexOptions.Compiled); // Noncompliant
            Regex.Matches("", "{foo}{bar}", RegexOptions.Compiled, TimeSpan.Zero); // Noncompliant

            Regex.Replace(s, "ab*cd*", match => ""); // Noncompliant
            Regex.Replace("", "ab*cd*", ""); // Noncompliant
            Regex.Replace("", "ab*cd*", match => "", RegexOptions.Compiled); // Noncompliant
            Regex.Replace("", "ab*cd*", s, RegexOptions.Compiled); // Noncompliant
            Regex.Replace("", "ab*cd*", match => "", RegexOptions.Compiled, TimeSpan.Zero); // Noncompliant
            Regex.Replace("", "ab*cd*", "", RegexOptions.Compiled, TimeSpan.Zero); // Noncompliant
            Regex.Replace("", "ab\\*cd\\*", "", RegexOptions.Compiled, TimeSpan.Zero); // Noncompliant FP (escaped special characters)

            Regex.Split("", "a+a+"); // Noncompliant
            Regex.Split("", "a+a+", RegexOptions.Compiled); // Noncompliant
            Regex.Split("", "a+a+", RegexOptions.Compiled, TimeSpan.Zero); // Noncompliant

            new System.Text.RegularExpressions.Regex("a+a+"); // Noncompliant
            new RE("a+b+"); // Noncompliant
            System.Text.RegularExpressions.Regex.IsMatch("", "{}{}"); // Noncompliant
            RE.IsMatch("", "a**"); // Noncompliant
            IsMatch("", "b**"); // Noncompliant

            // Non-static methods are compliant
            r.IsMatch("a+a+");
            r.IsMatch("{ab}*{ab}+{cd}+foo*", 0);

            r.Match("{ab}*{ab}+{cd}+foo*");
            r.Match("{ab}*{ab}+{cd}+foo*", 0);
            r.Match("{ab}*{ab}+{cd}+foo*", 0, 1);

            r.Matches("{ab}*{ab}+{cd}+foo*");
            r.Matches("{ab}*{ab}+{cd}+foo*", 0);

            r.Replace("{ab}*{ab}+{cd}+foo*", match => "{ab}*{ab}+{cd}+foo*");
            r.Replace("{ab}*{ab}+{cd}+foo*", "{ab}*{ab}+{cd}+foo*");
            r.Replace("{ab}*{ab}+{cd}+foo*", match => "{ab}*{ab}+{cd}+foo*", 0);
            r.Replace("{ab}*{ab}+{cd}+foo*", "{ab}*{ab}+{cd}+foo*", 0);
            r.Replace("{ab}*{ab}+{cd}+foo*", match => "{ab}*{ab}+{cd}+foo*", 0, 0);
            r.Replace("{ab}*{ab}+{cd}+foo*", "{ab}*{ab}+{cd}+foo*", 0, 0);

            r.Split("{ab}*{ab}+{cd}+foo*");
            r.Split("{ab}*{ab}+{cd}+foo*", 0);
            r.Split("{ab}*{ab}+{cd}+foo*", 0, 0);

            // not hardcoded strings are compliant
            r = new Regex(s);
            r = new Regex(s, RegexOptions.Compiled, TimeSpan.Zero);
            Regex.Replace("{ab}*{ab}+{cd}+foo*", s, "{ab}*{ab}+{cd}+foo*", RegexOptions.Compiled, TimeSpan.Zero);
            Regex.Split("{ab}*{ab}+{cd}+foo*", s, RegexOptions.Compiled, TimeSpan.Zero);
        }
    }
}
