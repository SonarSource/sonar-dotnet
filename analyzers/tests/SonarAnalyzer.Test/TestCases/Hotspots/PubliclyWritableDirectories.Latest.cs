using System;
using System.IO;

namespace CSharp10
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

namespace CSharp11
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

        void Utf8StringLiterals()
        {
            var tmp = "%USERPROFILE%\\AppData\\Local\\Temp\\f"u8; // Noncompliant
            tmp = "%TEMP%\\f"u8;                // Noncompliant
            tmp = "/tmp/"u8;                    // Noncompliant
            tmp = "/tmp"u8;                     // Noncompliant
            tmp = "/var/tmp/f"u8;               // Noncompliant
            tmp = "/usr/tmp/f"u8;               // Noncompliant
            tmp = "/dev/shm/f"u8;               // Noncompliant
            tmp = "/dev/mqueue/f"u8;            // Noncompliant
            tmp = "/run/lock/f"u8;              // Noncompliant
            tmp = "/var/run/lock/f"u8;          // Noncompliant
            tmp = "/Library/Caches/f"u8;        // Noncompliant
            tmp = "/Users/Shared/f"u8;          // Noncompliant
            tmp = "/private/tmp/f"u8;           // Noncompliant
            tmp = "/private/var/tmp/"u8;        // Noncompliant
            tmp = "C:\\Windows\\Temp\\f"u8;     // Noncompliant
            tmp = "C:\\Temp\\f"u8;              // Noncompliant
            tmp = "C:\\TEMP\\f"u8;              // Noncompliant
            tmp = "C:\\TMP\\f"u8;               // Noncompliant
            tmp = @"/tmp/f"u8;                  // Noncompliant
            tmp = "D:\\Windows\\Temp\\f"u8;     // Noncompliant
            tmp = "\\\\Server_Name\\Temp\\f"u8; // Noncompliant
            tmp = "\\Windows\\Temp\\f"u8;       // Noncompliant
            tmp = @"C:\Windows\Temp\f"u8;       // Noncompliant
            tmp = @"D:\Windows\Temp\f"u8;       // Noncompliant
            tmp = @"\\Windows\Temp\f"u8;        // Noncompliant
            tmp = @"\Windows\Temp\f"u8;         // Noncompliant
        }
    }
}

namespace CSharp12
{
    class PrimaryConstructor(string ctorParam = "C:\\TMP\\f",        // Noncompliant
                             string ctorParam2 = $"C:\\{"TM"}P\\f")  // FN
    {
        void Method(string methodParam = "C:\\TMP\\f",        // Noncompliant
                    string methodParam2 = $"C:\\{"TM"}P\\f")  // FN
        {
            const string p = "p";
            var lambda = (string lambdaParam = "C:\\TMP\\f",                // Noncompliant
                          string lambdaParam2 = $"C:\\{"TM"}P\\f",          // FN
                          string lambdaParam3 = $"/tem{p}") => lambdaParam; // Noncompliant
        }
    }
}

namespace CSharp13
{
    class MyClass
    {
        void EscapeSequence()
        {
            var tmp = "\e%TEMP%\\f";                           // Compliant
            tmp = "\e/tmp";                                    // Compliant
            tmp = "/var\e/tmp/f";                              // Compliant
            tmp = "/usr/tm\ep/f";                              // Compliant
            tmp = "%USERPROFILE%\\AppData\\Local\\Temp\\f\e";  // Noncompliant
            tmp = "/tmp/\e";                                   // Noncompliant
            tmp = "/tmp/som\ething";                           // Noncompliant
            tmp = @"/tmp/f\e";                                 // Noncompliant
            tmp = "D:\\Windows\\Temp\\f\\\u001b";              // Noncompliant
            tmp = "\\\\\eServer_Name\\Temp\\f";                // Noncompliant
        }
    }
}
