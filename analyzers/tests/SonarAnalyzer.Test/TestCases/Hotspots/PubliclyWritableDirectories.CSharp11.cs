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
