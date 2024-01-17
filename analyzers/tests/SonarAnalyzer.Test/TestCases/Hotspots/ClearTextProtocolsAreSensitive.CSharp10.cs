using System;
using System.Net;
using System.Net.Mail;

const string protocol1 = "http://";
const string protocol2 = "https://";
const string address = "foo.com";
const string noncompliant = $"{protocol1}{address}"; // Noncompliant
const string compliant = $"{protocol2}{address}";

string nestedNoncompliant = $"{$"{protocol1}somtehing."}{address}"; // Noncompliant
// Noncompliant@-1

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
