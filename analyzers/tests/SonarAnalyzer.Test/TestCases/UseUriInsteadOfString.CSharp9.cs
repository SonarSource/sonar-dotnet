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

public record S3994
{
    public S3994(string uri) { }            // Noncompliant {{Either change this parameter type to 'System.Uri' or provide an overload which takes a 'System.Uri' parameter.}}

    public S3994(string uri, bool blah) { } // Noncompliant {{Either change this parameter type to 'System.Uri' or provide an overload which takes a 'System.Uri' parameter.}}

    public virtual string Url { get; set; } // Noncompliant {{Change this property type to 'System.Uri'.}}
//                 ^^^^^^
}

public record WithParams(string uri) // Noncompliant {{Change this property type to 'System.Uri'.}}
{
    public WithParams(string uri, bool somethingElse) : this(uri) { } // Noncompliant {{Either change this parameter type to 'System.Uri' or provide an overload which takes a 'System.Uri' parameter.}}
}
