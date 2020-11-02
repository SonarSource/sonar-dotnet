S3994 p = new("www.sonarsource.com"); // Compliant - FN

string GetUrl(string url) => ""; // Compliant - FN

public record S3994
{
    public S3994(string uri) { } // Noncompliant {{Either change this parameter type to 'System.Uri' or provide an overload which takes a 'System.Uri' parameter.}}

    public S3994(string uri, bool blah) { } // Noncompliant {{Either change this parameter type to 'System.Uri' or provide an overload which takes a 'System.Uri' parameter.}}

    public virtual string Url { get; set; } // Noncompliant {{Change this property type to 'System.Uri'.}}
//                 ^^^^^^
}
