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
        int ThisIsAnUrlProperty { get; set; } // Compliant
        int ThisIsAnUrlMethod() => 1; // Compliant


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

        string OverloadedMethod(string url) => ""; // Compliant
        string OverloadedMethod(Uri url) => ""; // Compliant

        string ParamTest(string uri) => ""; // Noncompliant {{Either change this parameter type to 'System.Uri' or provide an overload which takes a 'System.Uri' parameter.}}
//                       ^^^^^^
        string ParamTest2(int url) => ""; // Compliant
        string ParamTestOverload(string uriParam) => ""; // Noncompliant {{Either change this parameter type to 'System.Uri' or provide an overload which takes a 'System.Uri' parameter.}}
//                               ^^^^^^
        string ParamTestOverload(int uriParam) => ""; // Compliant
        string MultipleParams(string uriParam, object urnParam, string urlParam, string andThisIsFine) => "";
//                            ^^^^^^ Noncompliant {{Either change this parameter type to 'System.Uri' or provide an overload which takes a 'System.Uri' parameter.}}
//                                                              ^^^^^^ Noncompliant@-1 {{Either change this parameter type to 'System.Uri' or provide an overload which takes a 'System.Uri' parameter.}}

        string AllUrlWrong(string url, string uri, string urn) => "";
//      ^^^^^^ Noncompliant {{Change this return type to 'System.Uri'.}}
//                         ^^^^^^ Noncompliant@-1 {{Either change this parameter type to 'System.Uri' or provide an overload which takes a 'System.Uri' parameter.}}
//                                     ^^^^^^ Noncompliant@-2 {{Either change this parameter type to 'System.Uri' or provide an overload which takes a 'System.Uri' parameter.}}
//                                                 ^^^^^^ Noncompliant@-3 {{Either change this parameter type to 'System.Uri' or provide an overload which takes a 'System.Uri' parameter.}}

    }
}
