using System;

namespace CSharp13
{
    partial class S3996
    {
        partial string Url { get; set; } // Noncompliant {{Change the 'Url' property type to 'System.Uri'.}}
//              ^^^^^^
        partial string url // Noncompliant {{Change the 'url' property type to 'System.Uri'.}}
//              ^^^^^^
        {
            get;
            set;
        }

        partial string FooUrlBar { get; set; } // Noncompliant {{Change the 'FooUrlBar' property type to 'System.Uri'.}}
        partial int ThisIsAnUrlProperty { get; set; } // Compliant

        // Urn
        partial string Urn { get; set; } // Noncompliant {{Change the 'Urn' property type to 'System.Uri'.}}
//              ^^^^^^
        partial string FooUrnBar { get; set; } // Noncompliant {{Change the 'FooUrnBar' property type to 'System.Uri'.}}

        // Uri
        partial string Uri { get; set; } // Noncompliant {{Change the 'Uri' property type to 'System.Uri'.}}
//              ^^^^^^
        partial string FooUriBar { get; set; } // Noncompliant {{Change the 'FooUriBar' property type to 'System.Uri'.}}

        partial string Urn2 { get; set; } // Noncompliant

        // Test there are no false positives
        partial string TurnOff { get; set; } // Compliant
        partial string Urinal { get; set; } // Compliant
        partial string Hourly { get; set; } // Compliant
        partial string urifoo { get; set; } // Compliant
        partial string urlfoo { get; set; } // Compliant
        partial string urnfoo { get; set; } // Compliant
    }
}
