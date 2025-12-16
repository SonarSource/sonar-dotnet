using System;
using System.Net;
using System.Net.Mail;

public class Usings
{
    public void Method()
    {
        using var wc = new WebClient();
        wc.DownloadData("http://foo.com"); // Noncompliant
        wc.DownloadData("https://foo.com");

        using var notSet = new SmtpClient("host", 25); // Noncompliant {{EnableSsl should be set to true.}}
        using var constructorFalse = new SmtpClient("host", 25) { EnableSsl = false }; // Noncompliant

        using var constructor42 = new SmtpClient("host", 25) { EnableSsl = 42 }; // Error [CS0029] Cannot implicitly convert type 'int' to 'bool'
                                                                                 // Noncompliant@-1 FP

        using var localhosting = new SmtpClient("localhosting", 25); // Noncompliant
        using var localhost = new SmtpClient("localhost", 25); // Compliant due to well known value
        using var loopback = new SmtpClient("127.0.0.1", 25); // Compliant due to well known value
        using var constructorTrue = new SmtpClient("host", 25) { EnableSsl = true };

        using var propertyTrue = new SmtpClient("host", 25); // Compliant, property is set below
        propertyTrue.EnableSsl = true;

        using var propertyFalse = new SmtpClient("host", 25); // Noncompliant {{EnableSsl should be set to true.}}
        propertyFalse.EnableSsl = false;

        using var setReset = new SmtpClient("host", 25) { EnableSsl = true }; // FN - it is later set to false
        setReset.EnableSsl = false;
    }
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

public static class Extensions
{
    extension (string s)
    {
        public string CompliantProp => "https://foo.com";       // Compliant
        public string NonCompliantProp => "http://foo.com";     // Noncompliant

        public string CompliantMethod() => "https://foo.com";   // Compliant
        public string NonCompliantMethod() => "http://foo.com";   // Noncompliant
    }
}

public class FieldKeyword
{
    public string Compliant { get { return "https://foo.com" + field; } set { field = "https://foo.com" + value; } }    // Compliant
    public string NonCompliant
    {
        get { return "http://foo.com" + field; }    // Noncompliant
        set { field = "http://foo.com" + value; } } // Noncompliant
}

public class NullConditionalAssignment
{
    public class Sample
    {
        public string Url { get; set; }
    }
    public void SomeMethod(Sample sample)
    {
        sample?.Url = "https://foo.com";  // Compliant
        sample?.Url = "http://foo.com";  // Noncompliant
    }
}
