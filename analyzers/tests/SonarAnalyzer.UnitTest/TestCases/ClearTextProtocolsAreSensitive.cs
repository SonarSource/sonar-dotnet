using System;
using System.Collections.Generic;
using System.IO;
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

        private string UntrackedSource() => string.Empty;

        public void TelnetExample() // This line is compliant, even when it contains "Telnet" keyword
        {
            // Method names
            var a1 = Telnet(); // Noncompliant
            var a2 = TelnetClient(); // Noncompliant
            var a3 = TcpTelnet40(); // Noncompliant
            var a4 = TcpTelnetClient(); // Noncompliant
            var a5 = Tcp_Telnet_Client(); // Noncompliant

            // Namespaces
            var b1 = new Company.Telnet.Client(); // Noncompliant
            var b2 = new Telnet.Stream(); // Noncompliant
            var b3 = new Company.TcpTelnetClient.Implementation(); // Noncompliant
            var b4 = Company.TcpTelnetClient.Implementation.Connect(); // Noncompliant

            // Types
            var c1 = new ClassNames.Telnet(); // Noncompliant
            var c2 = new ClassNames.TelnetClient(); // Noncompliant
            var c3 = new ClassNames.TcpTelnetClient(); // Noncompliant

            // Method names
            var d1 = TelNet();
            var d2 = Telnetwork();
            var d3 = HotelNetwork();

            // Namespaces
            var e1 = new Company.TelNet.Client();
            var e2 = new TelNet.Stream();
            var e3 = new Company.TcpTelnetwork.Implementation();
            var e4 = Company.TcpTelnetwork.Implementation.Connect();

            // Types
            var f1 = new ClassNames.TelNet();
            var f2 = new ClassNames.TelNet();
            var f3 = new ClassNames.HotelNetwork();
        }

        private static Stream Telnet() => null;
        private Stream TelnetClient() => null;
        private Stream TcpTelnet40() => null; // With protocol version
        private Stream TcpTelnetClient() => null;
        private Stream Tcp_Telnet_Client() => null;
        private static Stream TelNet() => null;
        private Stream Telnetwork() => null;
        private Stream HotelNetwork() => null;

        private readonly List<string> links = new List<string>
        {
            "http://example.com", // Noncompliant
            "https://example.com"
        };
    }
}

namespace Company.Telnet
{
    public class Client { }
}

namespace Telnet
{
    public class Stream { }
}

namespace Company.TcpTelnetClient
{
    public class Implementation
    {
        public static Stream Connect() => null;
    }
}

namespace ClassNames
{
    public class Telnet { }
    public class TelnetClient { }
    public class TcpTelnetClient { }
}

namespace Company.TelNet
{
    public class Client { }
}

namespace TelNet
{
    public class Stream { }
}

namespace Company.TcpTelnetwork
{
    public class Implementation
    {
        public static Stream Connect() => null;
    }
}

namespace ClassNames
{
    public class TelNet { }
    public class Telnetwork { }
    public class HotelNetwork { }
}
