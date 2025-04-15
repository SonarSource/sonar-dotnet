using System;

namespace CSharp9
{
    public record S3994
    {
        public S3994(string uri) { }            // Noncompliant {{Either change this parameter type to 'System.Uri' or provide an overload which takes a 'System.Uri' parameter.}}

        public S3994(string uri, bool blah) { } // Noncompliant {{Either change this parameter type to 'System.Uri' or provide an overload which takes a 'System.Uri' parameter.}}

        public virtual string Url { get; set; } // Noncompliant {{Change the 'Url' property type to 'System.Uri'.}}
//                     ^^^^^^
    }

    public record WithParams(string uri) // Noncompliant {{Change the 'uri' property type to 'System.Uri'.}}
//                           ^^^^^^^^^^
    {
        public WithParams(string uri, bool somethingElse) : this(uri) { } // Noncompliant {{Either change this parameter type to 'System.Uri' or provide an overload which takes a 'System.Uri' parameter.}}
    }

    public record WithTwoStringParams(string uri, bool condition, string anotherUri);
    //                                ^^^^^^^^^^{{Change the 'uri' property type to 'System.Uri'.}}
    //                                                            ^^^^^^^^^^^^^^^^^@-1{{Change the 'anotherUri' property type to 'System.Uri'.}}
}
namespace CSharp10
{
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

    public record struct WithParams(string uri) // Noncompliant {{Change the 'uri' property type to 'System.Uri'.}}
    {
        public WithParams(string uri, bool somethingElse) : this(uri) { } // Noncompliant {{Either change this parameter type to 'System.Uri' or provide an overload which takes a 'System.Uri' parameter.}}
    }
}

namespace CSharp11
{
    public class Foo
    {
        public Foo(string uri) { }
        public Foo(Uri uri) { }

        public void Method(string uri) => Method(new Uri(uri));
        public void Method(Uri uri) { }

        public void Test()
        {
            new Foo("""www.sonarsource.com""");              // Noncompliant {{Call the overload that takes a 'System.Uri' as an argument instead.}}
            new Foo("""
            www.sonarsource.com
            """);                                        // Noncompliant@-2
            new Foo($$"""
                www.sonarsource{{
                1 + 1
                }}.com
                """);                                        // Noncompliant@-4
            Method("""www.sonarsource.com""");               // Noncompliant
            Method($$"""www.sonarsource{{1 + 1}}.com""");    // Noncompliant
            Method($"www.sonarsource{                        // Noncompliant
                    1 + 1}.com");
        }
    }
}

namespace CSharp13
{
    partial class S3996
    {
        partial string Url { get => ""; set { } } // Noncompliant {{Change the 'Url' property type to 'System.Uri'.}}
//              ^^^^^^
        partial string url { get => ""; set { } }// Noncompliant {{Change the 'url' property type to 'System.Uri'.}}
//              ^^^^^^

        partial string FooUrlBar { get => ""; set { } } // Noncompliant {{Change the 'FooUrlBar' property type to 'System.Uri'.}}
        partial int ThisIsAnUrlProperty { get => 42; set { } } // Compliant


        // Urn
        partial string Urn { get => ""; set { } } // Noncompliant {{Change the 'Urn' property type to 'System.Uri'.}}
//              ^^^^^^
        partial string FooUrnBar { get => ""; set { } } // Noncompliant {{Change the 'FooUrnBar' property type to 'System.Uri'.}}

        // Uri
        partial string Uri { get => ""; set { } } // Noncompliant {{Change the 'Uri' property type to 'System.Uri'.}}
//              ^^^^^^
        partial string FooUriBar { get => ""; set { } } // Noncompliant {{Change the 'FooUriBar' property type to 'System.Uri'.}}

        partial string Urn2 // Noncompliant
        {
            get => "";
            set => throw new NotSupportedException();
        }

        // Test there are no false positives
        partial string TurnOff { get => ""; set { } } // Compliant
        partial string Urinal { get => ""; set { } } // Compliant
        partial string Hourly { get => ""; set { } } // Compliant
        partial string urifoo { get => ""; set { } } // Compliant
        partial string urlfoo { get => ""; set { } } // Compliant
        partial string urnfoo { get => ""; set { } } // Compliant
    }
}
