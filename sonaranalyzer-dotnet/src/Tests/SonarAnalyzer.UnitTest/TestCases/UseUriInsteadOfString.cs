using System;

namespace Tests.Diagnostics
{
    class Foo
    {
        string Url { get; set; } // Noncompliant {{Change this property type to 'System.Uri'.}}
//      ^^^^^^

        string Uri() => ""; // Noncompliant {{Change this return type to 'System.Uri'.}}
//      ^^^^^^
    }

    class Bar : Foo
    {
        override string Url { get; set; } // Compliant
        override string Uri() => ""; // Compliant
    }

    class Program
    {
        // Url
        string Url { get; set; } // Noncompliant {{Change this property type to 'System.Uri'.}}
//      ^^^^^^
        string url // Noncompliant {{Change this property type to 'System.Uri'.}}
//      ^^^^^^
        {
            get;
            set;
        }
        string Url2 => ""; // Noncompliant {{Change this property type to 'System.Uri'.}}
//      ^^^^^^
        string FooUrlBar{ get; set; } // Noncompliant {{Change this property type to 'System.Uri'.}}
        string AUrlMethod() => ""; // Noncompliant {{Change this return type to 'System.Uri'.}}
        string UrlMethod() => ""; // Noncompliant {{Change this return type to 'System.Uri'.}}


        // Urn
        string Urn { get; set; } // Noncompliant {{Change this property type to 'System.Uri'.}}
//      ^^^^^^
        string FooUrnBar{ get; set; } // Noncompliant {{Change this property type to 'System.Uri'.}}
        string AUrnMethod() => ""; // Noncompliant {{Change this return type to 'System.Uri'.}}
        string UrnMethod() => ""; // Noncompliant {{Change this return type to 'System.Uri'.}}


        // Uri
        string Uri { get; set; } // Noncompliant {{Change this property type to 'System.Uri'.}}
//      ^^^^^^
        string FooUriBar{ get; set; } // Noncompliant {{Change this property type to 'System.Uri'.}}
        string AUriMethod() => ""; // Noncompliant {{Change this return type to 'System.Uri'.}}
        string UriMethod() => ""; // Noncompliant {{Change this return type to 'System.Uri'.}}


        // Test there are no false positives
        string TurnOff { get; set; } // Compliant
        string Urinal { get; set; } // Compliant
        string Hourly { get; set; } // Compliant
        string Pouring() => ""; // Compliant
        string Hurl() => ""; // Compliant
        string urifoo { get; set; } // Compliant
        string urlfoo { get; set; } // Compliant
        string urnfoo { get; set; } // Compliant
    }
}
