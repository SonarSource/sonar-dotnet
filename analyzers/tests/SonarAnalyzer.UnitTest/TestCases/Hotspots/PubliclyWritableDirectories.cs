using System;
using System.IO;

namespace Tests.Diagnostics
{
    public class Program
    {
        public void Compliant()
        {
            var randomPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()); // Compliant

            using (var tmpDir = new StreamReader(@"Path is /tmp/f")) { };  // Compliant
            using (var tmpDir = new StreamReader(@"C:\\Windows\\Tempete")) { };  // Compliant;
            var url = "http://example.domain/tmp/f"; // Compliant
        }

        private string CompliantWithArg(string dir = "/other/tmp") // Compliant
        {
            return dir;
        }

        private class InnerClass
        {
            public string StringProp { get; set; }
        }

        public void v(string partOfPath)
        {
            // Environment
            var tmp = Path.GetTempPath(); // Noncompliant
            tmp = Path.GetTempPath(); // Noncompliant
            InnerClass inner = new InnerClass() { StringProp = Path.GetTempPath() }; // Noncompliant
            tmp = Environment.GetEnvironmentVariable("TMPDIR"); // Noncompliant {{Make sure publicly writable directories are used safely here.}}
//                ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            tmp = Environment.GetEnvironmentVariable("TMP"); // Noncompliant
            tmp = Environment.GetEnvironmentVariable("TEMP"); // Noncompliant
            tmp = Environment.ExpandEnvironmentVariables("%TMPDIR%"); // Noncompliant
            tmp = Environment.ExpandEnvironmentVariables("%TMP%"); // Noncompliant
            tmp = Environment.ExpandEnvironmentVariables("%TEMP%"); // Noncompliant
            tmp = "%USERPROFILE%\\AppData\\Local\\Temp\\f"; // Noncompliant
            tmp = "%TEMP%\\f"; // Noncompliant
            tmp = "%TMP%\\f"; // Noncompliant
            tmp = "%TMPDIR%\\f"; // Noncompliant
//                ^^^^^^^^^^^^^

            // Common
            using (var tmpDir = new StreamReader("/tmp/f")) { }; // Noncompliant
            using (var tmpDir = new StreamReader("/tmp")) { }; // Noncompliant
            using (var tmpDir = new StreamReader("/var/tmp/f")) { }; // Noncompliant
            using (var tmpDir = new StreamReader("/usr/tmp/f")) { }; // Noncompliant
            using (var tmpDir = new StreamReader("/dev/shm/f")) { }; // Noncompliant

            // Linux
            using (var tmpDir = new StreamReader("/dev/mqueue/f")) { }; // Noncompliant
            using (var tmpDir = new StreamReader("/run/lock/f")) { }; // Noncompliant
            using (var tmpDir = new StreamReader("/var/run/lock/f")) { }; // Noncompliant

            // MacOS
            using (var tmpDir = new StreamReader("/Library/Caches/f")) { }; // Noncompliant
            using (var tmpDir = new StreamReader("/Users/Shared/f")) { }; // Noncompliant
            using (var tmpDir = new StreamReader("/private/tmp/f")) { }; // Noncompliant
            using (var tmpDir = new StreamReader("/private/var/tmp/f")) { }; // Noncompliant

            // Windows
            using (var tmpDir = new StreamReader("C:\\Windows\\Temp\\f")) { }; // Noncompliant
            using (var tmpDir = new StreamReader("C:\\Temp\\f")) { }; // Noncompliant
            using (var tmpDir = new StreamReader("C:\\TEMP\\f")) { }; // Noncompliant
            using (var tmpDir = new StreamReader("C:\\TMP\\f")) { }; // Noncompliant

            // Variates
            using (var tmpDir = new StreamReader(@"/tmp/f")) { };  // Noncompliant
            using (var tmpDir = new StreamReader("D:\\Windows\\Temp\\f")) { }; // Noncompliant
            using (var tmpDir = new StreamReader("\\\\Windows\\Temp\\f")) { }; // Noncompliant
            using (var tmpDir = new StreamReader("\\Windows\\Temp\\f")) { }; // Noncompliant
            using (var tmpDir = new StreamReader(@"C:\Windows\Temp\f")) { }; // Noncompliant
            using (var tmpDir = new StreamReader(@"D:\Windows\Temp\f")) { }; // Noncompliant
            using (var tmpDir = new StreamReader(@"\\Windows\Temp\f")) { }; // Noncompliant
            using (var tmpDir = new StreamReader(@"\Windows\Temp\f")) { }; // Noncompliant
            tmp = "/tmp/f"; // Noncompliant
            tmp = "/tEmP/f"; // Noncompliant

            // Interpolated
            tmp = $"/tmp/{partOfPath}"; // Noncompliant
//                ^^^^^^^^^^^^^^^^^^^^

        }

        private string NonCompliantWithArg(string dir = "/tmp") // Noncompliant
        {
            return dir;
        }
    }
}
