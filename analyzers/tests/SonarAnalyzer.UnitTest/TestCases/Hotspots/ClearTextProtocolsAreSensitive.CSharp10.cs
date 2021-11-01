using System;
using System.Net;
using System.Net.Mail;

const string protocol1 = "http://";
const string protocol2 = "https://";
const string address = "foo.com";
const string noncompliant = $"{protocol1}{address}"; // FN
const string compliant = $"{protocol2}{address}";

public record struct RecordStruct
{
    public string Address { get; init; } = "http://foo.com"; // Noncompliant

    public void Method()
    {
        new TelnetRecordStruct(); // Noncompliant {{Using telnet protocol is insecure. Use ssh instead.}}
    }
}

public record struct TelnetRecordStruct { }
