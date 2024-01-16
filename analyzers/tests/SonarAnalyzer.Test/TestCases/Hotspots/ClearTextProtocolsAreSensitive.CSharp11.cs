using System;

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
