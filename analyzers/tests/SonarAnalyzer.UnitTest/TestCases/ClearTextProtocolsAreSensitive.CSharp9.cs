using System;
using System.Net;
using System.Net.Mail;

const string a = "http://example.com"; // Noncompliant {{Using http protocol is insecure. Use https instead.}}
const string b = "https://example.com";

const string e = @"telnet://anonymous@example.com"; // Noncompliant {{Using telnet protocol is insecure. Use ssh instead.}}
const string f = @"ssh://anonymous@example.com";

string i = $"ftp://anonymous@example.com"; // Noncompliant {{Using ftp protocol is insecure. Use sftp, scp or ftps instead.}}
string l = $"ftp://anonymous@127.0.0.1";

void Method()
{
    var a = "http://example.com"; // Noncompliant
    var b = "https://example.com";
    var httpProtocolScheme = "http://"; // It's compliant when standalone

    var uri = new Uri("http://example.com"); // Noncompliant
    var uriSafe = new Uri("https://example.com");

    using var wc = new WebClient();
    wc.DownloadData("http://example.com"); // Noncompliant
    wc.DownloadData("https://example.com");
}

public record Record
{
    public string Address { get; init; } = "http://example.com"; // Noncompliant

    public void Method()
    {
        using var notSet = new SmtpClient("host", 25); // Noncompliant {{EnableSsl should be set to true.}}
        using SmtpClient targetNew = new ("host", 25); // Noncompliant
        using SmtpClient targetNewWithInitializer = new("host", 25) {EnableSsl = false }; // Noncompliant
    }
}
