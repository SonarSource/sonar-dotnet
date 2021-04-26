using System;
using System.Net;
using System.Net.Mail;

const string a = "http://foo.com"; // Noncompliant {{Using http protocol is insecure. Use https instead.}}
const string b = "https://foo.com";

const string e = @"telnet://anonymous@foo.com"; // Noncompliant {{Using telnet protocol is insecure. Use ssh instead.}}
const string f = @"ssh://anonymous@foo.com";

string i = $"ftp://anonymous@foo.com"; // Noncompliant {{Using ftp protocol is insecure. Use sftp, scp or ftps instead.}}
string l = $"ftp://anonymous@127.0.0.1";

void Method()
{
    var a = "http://foo.com"; // Noncompliant
    var b = "https://foo.com";
    var httpProtocolScheme = "http://"; // It's compliant when standalone

    var uri = new Uri("http://foo.com"); // Noncompliant
    var uriSafe = new Uri("https://foo.com");

    using var wc = new WebClient();
    wc.DownloadData("http://foo.com"); // Noncompliant
    wc.DownloadData("https://foo.com");
}

public record Record
{
    public string Address { get; init; } = "http://foo.com"; // Noncompliant

    public void Method()
    {
        using var notSet = new SmtpClient("host", 25); // Noncompliant {{EnableSsl should be set to true.}}
        using SmtpClient targetNew = new ("host", 25); // FN
        using SmtpClient targetNewWithInitializer = new("host", 25) {EnableSsl = false }; // FN

        new TelnetRecord(); // Noncompliant {{Using telnet protocol is insecure. Use ssh instead.}}
    }
}

public record TelnetRecord { }
