using System;
using System.IO;

namespace Tests.Diagnostics
{
    public class Program
    {
        void RawStringLiterals()
        {
            var tmp = Environment.GetEnvironmentVariable("""TMPDIR"""); // Noncompliant
        }

        void NewlinesInStringInterpolation(string firstPartOfPath, string secondPartOfPath)
        {
            string dir = $"/tmp/{firstPartOfPath + // Noncompliant
                secondPartOfPath}";
            string dirRawString = $$"""/tmp/{{firstPartOfPath + // Noncompliant
                            secondPartOfPath}}""";
        }

        void Utf8StringLiterals(string firstPartOfPath, string secondPartOfPath)
        {
            var tmp = "%USERPROFILE%\\AppData\\Local\\Temp\\f"u8; // Noncompliant
            tmp = "\u0025\u0055\u0053\u0045\u0052\u0050\u0052\u004f\u0046\u0049\u004c\u0045\u0025\u005c\u0041\u0070\u0070\u0044\u0061\u0074\u0061\u005c\u004c\u006f\u0063\u0061\u006c\u005c\u0054\u0065\u006d\u0070\u005c\u0066"u8; // Noncompliant
            tmp = "%TEMP%\\f"u8; // Noncompliant
        }

    }
}
