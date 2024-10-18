using System;

Foo p = new("www.sonarsource.com");        // Noncompliant

Foo q = new("www.sonarsource.com", false); // Compliant

string GetUrl(string url) => "";           // Compliant - FN

public record Foo
{
    public Foo(string uri) { }

    public Foo(string uri, bool blah) { }   // Noncompliant {{Either change this parameter type to 'System.Uri' or provide an overload which takes a 'System.Uri' parameter.}}

    public Foo(Uri uri) { }                 // Compliant
}
