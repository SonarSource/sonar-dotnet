using System;
using System.Net;
using System.Net.Mail;

const string a = "http://foo.com"; // Noncompliant {{Using http protocol is insecure. Use https instead.}}
const string b = "https://foo.com";

const string e = @"telnet://anonymous@foo.com"; // Noncompliant {{Using telnet protocol is insecure. Use ssh instead.}}
const string f = @"ssh://anonymous@foo.com";

string i = $"ftp://anonymous@foo.com"; // Noncompliant {{Using ftp protocol is insecure. Use sftp, scp or ftps instead.}}
string l = $"ftp://anonymous@127.0.0.1";

const string protocol1 = "http://";
const string protocol2 = "https://";
const string address = "foo.com";
const string noncompliant = $"{protocol1}{address}"; // Noncompliant
const string compliant = $"{protocol2}{address}";

string nestedNoncompliant = $"{$"{protocol1}somtehing."}{address}"; // Noncompliant
// Noncompliant@-1

void Method()
{
    var a = "http://foo.com"; // Noncompliant
    var b = "https://foo.com";
    var httpProtocolScheme = "http://"; // It's compliant when standalone
    Telnet c = new(); // Noncompliant
    Telnet d;
    d = new();        // Noncompliant

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
        using SmtpClient targetNew = new ("host", 25); // Noncompliant
        using SmtpClient targetNewWithInitializer = new("host", 25) {EnableSsl = false }; // Noncompliant

        new TelnetRecord(); // Noncompliant {{Using telnet protocol is insecure. Use ssh instead.}}
    }
}

public record TelnetRecord { }

public class Telnet { }

public record struct RecordStruct
{
    public string Address { get; init; } = "http://foo.com"; // Noncompliant

    public RecordStruct() { }

    public void Method()
    {
        new TelnetRecordStruct(); // Noncompliant {{Using telnet protocol is insecure. Use ssh instead.}}
    }

    public void SetValueAfterObjectInitialization()
    {
        var propertyTrue = new SmtpClient("host", 25); // Compliant, property is set below
        (propertyTrue.EnableSsl, var x1) = (true, 0);

        var propertyFalse = new SmtpClient("host", 25); // Noncompliant
        (propertyFalse.EnableSsl, var x2) = (false, 0);
    }
}

public record struct TelnetRecordStruct { }

public class CSharp11
{
    void RawStringLiterals()
    {
        const string protocol1 = """http://""";
        const string protocol2 = """https://""";
        const string address = """foo.com""";
        const string noncompliant = $"""{protocol1}{address}"""; // Noncompliant
        const string compliant = $"""{protocol2}{address}""";

        const string a = """http://foo.com"""; // Noncompliant {{Using http protocol is insecure. Use https instead.}}
    }

    void Utf8StringLiterals()
    {
        var b = "http://foo.com"u8; // Noncompliant
        var c = """http://foo.com"""u8; // Noncompliant
        var d = """
    http://foo.com
    """u8; // Noncompliant@-2
    }

    void NewlinesInStringInterpolation()
    {
        const string protocol1 = "http://";
        const string protocol2 = "https://";
        const string address = "foo.com";
        const string noncompliant = $"{protocol1 + // Noncompliant
            address}";
        const string compliant = $"{protocol2 +
            address}";
    }
}

class PrimaryConstructor(string ctorParam = "http://foo.com") // Noncompliant
{
    void Method(string methodParam = "http://foo.com") // Noncompliant
    {
        var lambda = (string lambdaParam = "http://foo.com") => lambdaParam; // Noncompliant
    }
}

public class Telnet2 { }

public sealed class Repro
{
    public Telnet2[] GetTelnet() => CreateTelnets();

    private static Telnet2[] CreateTelnets() => [new()]; // Noncompliant
}

public class CSharp13
{
    // https://sonarsource.atlassian.net/browse/NET-450
    void EscapeSequence()
    {
        _ = "http:/\e/example.com";                 // FN
        _ = "ftp://anonymous@example.com\e";        // FN
        _ = "ftp://anonymo\eus@example.com";        // FN
        _ = "telnet://anonymous@example.com\u001b"; // FN
        _ = "ftp://anonymous@\e";                   // Noncompliant
        _ = "ftp://anonymous@" + '\e';              // Noncompliant
    }
}
