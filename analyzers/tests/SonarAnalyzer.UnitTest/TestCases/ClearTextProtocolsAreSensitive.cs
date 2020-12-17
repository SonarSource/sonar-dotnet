using System;
using System.Net;

namespace Tests.Diagnostics
{
    class ClearTextProtocols
    {
        private const string a = "http://example.com"; // Noncompliant {{Using http protocol is insecure. Use https instead.}}
        private const string c = @"telnet://anonymous@example.com"; // Noncompliant {{Using telnet protocol is insecure. Use ssh instead.}}
        private readonly string b = $"ftp://anonymous@example.com"; // Noncompliant {{Using ftp protocol is insecure. Use sftp, scp or ftps instead.}}

        public void Method(string part)
        {
            var a = "http://example.com"; // Noncompliant
            var b = $"ftp://anonymous@example.com"; // Noncompliant
            var c = $"ftp://{part}@example.com"; // Noncompliant
            var d = @"telnet://anonymous@example.com"; // Noncompliant
            var e = "http://localservername"; // Noncompliant, it's not a "localhost"

            var uri = new Uri("http://example.com"); // Noncompliant

            using var wc = new WebClient();
            wc.DownloadData("http://example.com"); // Noncompliant
        }
    }
}
