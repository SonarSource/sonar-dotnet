using System;
using System.Net;
using System.Net.Mail;

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
            var b = "https://example.com";
            var c = "http://localservername"; // Noncompliant, it's not a "localhost"
            var d = "See http://www.example.com for more information";
            var httpProtocolScheme = "http://"; // It's compliant when standalone

            var e = $"ftp://anonymous@example.com"; // Noncompliant
            var f = $"ftp://{part}@example.com"; // Noncompliant
            var g = "ssh://anonymous@example.com";
            var h = "sftp://anonymous@example.com";
            var ftpProtocolScheme = "ftp://"; // It's compliant when standalone

            var i = @"telnet://anonymous@example.com"; // Noncompliant
            var telnetProtocolScheme = "telnet://"; // It's compliant when standalone

            var uri = new Uri("http://example.com"); // Noncompliant
            var uriSafe = new Uri("https://example.com");

            using var wc = new WebClient();
            wc.DownloadData("http://example.com"); // Noncompliant
            wc.DownloadData("https://example.com");
        }

        public void Smtp()
        {
            using var notSet = new SmtpClient("host", 25); // Noncompliant {{EnableSsl should be set to true.}}
            using var constructorFalse = new SmtpClient("host", 25) { EnableSsl = false }; // Noncompliant

            using var localhost = new SmtpClient("localhost", 25); // Compliant due to well known value
            using var loopback = new SmtpClient("127.0.0.1", 25); // Compliant due to well known value
            using var constructorTrue = new SmtpClient("host", 25) { EnableSsl = true };

            using var propertyTrue = new SmtpClient("host", 25); // Compliant, property is set below
            propertyTrue.EnableSsl = true;

            using var propertyFalse = new SmtpClient("host", 25); // Noncompliant {{EnableSsl should be set to true.}}
            propertyFalse.EnableSsl = false;
        }

        public void Ftp()
        {
            var notSet = (FtpWebRequest)WebRequest.Create(UntrackedSource()); // Compliant, FN

            var setToFalse = (FtpWebRequest)WebRequest.Create(UntrackedSource());
            setToFalse.EnableSsl = false; // Noncompliant {{EnableSsl should be set to true.}}

            var setToTrue = (FtpWebRequest)WebRequest.Create(UntrackedSource());
            setToTrue.EnableSsl = true;
        }
    }
}
