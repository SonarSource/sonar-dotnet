using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

class Compliant
{
    void Ctor()
    {
        var defaultOrder = new Regex("valid pattern", RegexOptions.None); // Compliant

        var namedArgs = new Regex(
            options: RegexOptions.None,
            pattern: "valid pattern");

        var noRegex = new NoRegex("[A", RegexOptions.None); // Compliant
    }

    void Static()
    {
        var isMatch = Regex.IsMatch("some input", "valid pattern", RegexOptions.None); // Compliant
        var noRegex = NoRegex.IsMatch("some input", "[A", RegexOptions.None); // Compliant
    }

    void Unknown(string unknown)
    {
        var regex = new NoRegex(unknown + "[A", RegexOptions.None); // Compliant
    }

    bool Multiline(string input)
    {
        return Regex.IsMatch(input,
            @"[a
              |b
              |d]");  // Compliant
    }

    bool ConcatanationMultiline(string input)
    {
        return Regex.IsMatch(input, "[a"
            + "|b"
            + "|c"
            + "|d]"); // Compliant
    }

    bool ConcatanationSingleIne(string input)
    {
        return Regex.IsMatch(input, "a" + "|b" + "|c" + "|[A"); // Noncompliant
    }

    [RegularExpression("[0-9]+")] // Compliant
    public string Attribute { get; set; }
}

class Noncompliant
{
    private const string Prefix = ".*";

    void Ctor()
    {
        var patternOnly = new Regex("[A"); // Noncompliant
        //                          ^^^^

        var withConst = new Regex(Prefix + "[A"); // Noncompliant
    }

    void Static()
    {
        var isMatch = Regex.IsMatch("some input", "[A");    // Noncompliant
        //                                        ^^^^
        var match = Regex.Match("some input", "[A");        // Noncompliant
        var matches = Regex.Matches("some input", "[A");    // Noncompliant
        var split = Regex.Split("some input", "[A");        // Noncompliant

        var replace = Regex.Replace("some input", "[A", "some replacement"); // Noncompliant
    }

    bool Multiline(string input)
    {
        return Regex.IsMatch(input,
            @"|b
              |c
              |[A");  // Noncompliant @-2
    }

    bool ConcatanationMultiline(string input)
    {
        return Regex.IsMatch(input, "a" // Noncompliant
            + "|b"
            + "|c"
            + "|[A");
    }

    bool ConcatanationSingleIne(string input)
    {
        return Regex.IsMatch(input, "a" + "|b" + "|c" + "|[A"); // Noncompliant
    }

    [RegularExpression("[A")] // Noncompliant
    public string Attribute { get; set; }

    [System.ComponentModel.DataAnnotations.RegularExpression("[A")] // Noncompliant
    public string AttributeFullySpecified { get; set; }

    [global::System.ComponentModel.DataAnnotations.RegularExpression("[A")] // Noncompliant
    public string AttributeGloballySpecified { get; set; }
}

class DoesNotCrash
{
    bool UnknownVariable(string input)
    {
        return Regex.IsMatch(input, "a" + undefined); // Error [CS0103] The name 'undefined' does not exist in the current context
    }
}

public class NoRegex
{
    public NoRegex(string pattern, RegexOptions options) { }

    public static bool IsMatch(string input, string pattern, RegexOptions options) { return true; }
}
