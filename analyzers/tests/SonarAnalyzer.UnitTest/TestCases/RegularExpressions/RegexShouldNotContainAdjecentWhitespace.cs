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
        var patternOnly = new Regex("A  B"); // Noncompliant {{The pattern contains adjacent whitespace.}}
        //                          ^^^^^^
    }

    void Static()
    {
        var isMatch = Regex.IsMatch("some input", "A  B"); // Noncompliant
        //                                        ^^^^^^
        var match = Regex.Match("some input", "A  B"); // Noncompliant
        var matches = Regex.Matches("some input", "A  B"); // Noncompliant
        var replace = Regex.Replace("some input", "A  B", "some replacement"); // Noncompliant
        var split = Regex.Split("some input", "A  B"); // Noncompliant
    }

    [RegularExpression("A  B")] // Noncompliant
    public string Attribute { get; set; }
}
