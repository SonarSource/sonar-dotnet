using System;

namespace Tests.Diagnostics
{
    class Program
    {
        // Url
        string Url { get; set; } // Noncompliant {{Change this property type to 'System.Uri'}}
//      ^^^^^^
        string url // Noncompliant
//      ^^^^^^
        {
            get;
            set;
        }
        string Url2 => ""; // Noncompliant
//      ^^^^^^
        string urlSpelling{ get; set; } // Noncompliant
        string WellKnownUrl { get; set; } // Noncompliant
        string wellKnownUrl { get; set; } // Noncompliant
        string lastUrlZone{ get; set; } // Noncompliant
        string url2foo{ get; set; } // Noncompliant
        string foo2url { get; set; } // Noncompliant
        string url2 { get; set; } // Noncompliant

        string urlspelling { get; set; } // Compliant
        string UrlMethod() => ""; // Compliant


        // Urn
        string Urn { get; set; } // Noncompliant {{Change this property type to 'System.Uri'}}
//      ^^^^^^
        string urn { get; set; } // Noncompliant
        string urnSpelling { get; set; } // Noncompliant
        string WellKnownUrn { get; set; } // Noncompliant
        string wellKnownUrn { get; set; } // Noncompliant
        string lastUrnZone { get; set; } // Noncompliant
        string urn2foo { get; set; } // Noncompliant
        string foo2urn { get; set; } // Noncompliant
        string urn2 { get; set; } // Noncompliant

        string urnspelling { get; set; } // Compliant
        string UrnMethod() => ""; // Compliant


        // Uri
        string Uri { get; set; } // Noncompliant {{Change this property type to 'System.Uri'}}
//      ^^^^^^
        string uri { get; set; } // Noncompliant
        string uriSpelling { get; set; } // Noncompliant
        string WellKnownUri { get; set; } // Noncompliant
        string wellKnownUri { get; set; } // Noncompliant
        string lastUriZone { get; set; } // Noncompliant
        string uri2foo { get; set; } // Noncompliant
        string foo2uri { get; set; } // Noncompliant
        string uri2 { get; set; } // Noncompliant

        string UriMethod() => ""; // Compliant
        string urispelling { get; set; } // Compliant


        // Test there are no false positives
        string TurnOff { get; set; } // Compliant
        string Urinal { get; set; } // Compliant
        string Hourly { get; set; } // Compliant
        string Pouring { get; set; } // Compliant
        string Hurl { get; set; } // Compliant
    }
}
