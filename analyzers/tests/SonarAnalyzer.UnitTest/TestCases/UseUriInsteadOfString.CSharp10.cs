using System;

public record struct Foo
{
    public Foo(string uri) { }

    public Foo(string uri, bool blah) { }   // Noncompliant {{Either change this parameter type to 'System.Uri' or provide an overload which takes a 'System.Uri' parameter.}}

    public Foo(Uri uri) { }                 // Compliant
}

public record struct S3994
{
    public S3994(string uri) { }            // Noncompliant {{Either change this parameter type to 'System.Uri' or provide an overload which takes a 'System.Uri' parameter.}}

    public S3994(string uri, bool blah) { } // Noncompliant {{Either change this parameter type to 'System.Uri' or provide an overload which takes a 'System.Uri' parameter.}}
}

public record struct WithParams(string uri) // FN
{
    public WithParams(string uri, bool somethingElse) : this(uri) { } // Noncompliant {{Either change this parameter type to 'System.Uri' or provide an overload which takes a 'System.Uri' parameter.}}
}
