using System;
using System.Net;

namespace Tests.Diagnostics
{
    class ClearTextProtocols
    {
        private const string a = "http://example.com"; // Noncompliant {{Using http protocol is insecure. Use https instead.}}
        private const string b = "https://example.com";
        private const string c = "http://localhost";
        private const string d = "http://127.0.0.1";

        private const string e = @"telnet://anonymous@example.com"; // Noncompliant {{Using telnet protocol is insecure. Use ssh instead.}}
        private const string f = @"ssh://anonymous@example.com";
        private const string g = @"telnet://anonymous@localhost";
        private const string h = "telnet://anonymous@127.0.0.1";

        private readonly string i = $"ftp://anonymous@example.com"; // Noncompliant {{Using ftp protocol is insecure. Use sftp, scp or ftps instead.}}
        private readonly string j = $"sftp://anonymous@example.com";
        private readonly string k = $"ftp://anonymous@localhost";
        private readonly string l = $"ftp://anonymous@127.0.0.1";

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
