using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

class Compliant
{
    void Ctor()
    {
        var defaultOrder = new Regex("some pattern", RegexOptions.None); // Compliant

        var namedArgs = new Regex(
            options: RegexOptions.None,
            pattern: "some pattern");
    }

    void Static()
    {
        var isMatch = Regex.IsMatch("some input", "some pattern", RegexOptions.None); // Compliant
    }

    [RegularExpression("[0-9]+")] // Compliant
    public string Attribute { get; set; }
}

class Noncompliant
{
    void Ctor()
    {
        var patternOnly = new Regex("[A"); // Noncompliant
        //                          ^^^^
    }

    void Static()
    {
        var isMatch = Regex.IsMatch("some input", "[A"); // Noncompliant
        //                                        ^^^^
        var match = Regex.Match("some input", "[A"); // Noncompliant
        var matches = Regex.Matches("some input", "[A"); // Noncompliant
        var replace = Regex.Replace("some input", "[A", "some replacement"); // Noncompliant
        var split = Regex.Split("some input", "[A"); // Noncompliant
    }

    [RegularExpression("[A")] // Noncompliant
    public string Attribute { get; set; }

    [System.ComponentModel.DataAnnotations.RegularExpression("[A")] // Noncompliant
    public string AttributeFullySpecified { get; set; }

    [global::System.ComponentModel.DataAnnotations.RegularExpression("[A")] // Noncompliant
    public string AttributeGloballySpecified { get; set; }
}
