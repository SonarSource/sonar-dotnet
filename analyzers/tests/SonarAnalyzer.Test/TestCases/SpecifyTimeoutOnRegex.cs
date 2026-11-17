using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

class Compliant
{
    void Ctor()
    {
        var defaultOrder = new Regex("some pattern", RegexOptions.None, TimeSpan.FromSeconds(1)); // Compliant

        var namedArgs = new Regex(
            matchTimeout: TimeSpan.FromSeconds(1), // Compliant
            pattern: "some pattern",
            options: RegexOptions.None);
    }

    void Instance()
    {
        var regex = new Regex("some pattern", RegexOptions.None, TimeSpan.FromSeconds(1));

        var isMatch = regex.IsMatch("some input"); // Compliant
        var match = regex.Match("some input"); // Compliant
        var matches = regex.Matches("some input"); // Compliant
        var replace = regex.Replace("some input", "some replacement"); // Compliant
        var split = regex.Split("some input"); // Compliant
    }

    void Static()
    {
        var isMatch = Regex.IsMatch("some input", "some pattern", RegexOptions.None, TimeSpan.FromSeconds(1)); // Compliant
        var match = Regex.Match("some input", "some pattern", RegexOptions.None, TimeSpan.FromSeconds(1)); // Compliant
        var matches = Regex.Matches("some input", "some pattern", RegexOptions.None, TimeSpan.FromSeconds(1)); // Compliant
        var replace = Regex.Replace("some input", "some pattern", "some replacement", RegexOptions.None, TimeSpan.FromSeconds(1)); // Compliant
        var split = Regex.Split("some input", "some pattern", RegexOptions.None, TimeSpan.FromSeconds(1)); // Compliant
    }

    void NonBacktrackingSpecified()
    {
        var newOnBacktrackingOnly = new Regex("some pattern", (RegexOptions)1024); // Compliant
        var newOnAlsoBacktracking = new Regex("some pattern", (RegexOptions)1025); // Compliant. RegexOptions is a flag enum
        var staticOnSplit = Regex.Split("some input", "some pattern", (RegexOptions)1024); // Compliant
    }

    void NoRegex()
    {
        var match = NoSystem.Regex.IsMatch("some input", "some pattern"); // Compliant
    }

    [RegularExpression("[0-9]+")] // Compliant, Default timeout is 2000 ms.
    public string AttributeWithoutTimeout { get; set; }

    [RegularExpression("[0-9]+", MatchTimeoutInMilliseconds = 200)] // Compliant
    public string AttributeWithTimeout { get; set; }
}

class Noncompliant
{
    void Ctor()
    {
        var patternOnly = new Regex("a+b+"); // Noncompliant {{Pass a timeout to limit the execution time.}}
        //                ^^^^^^^^^^^^^^^^^
        var withOptions = new Regex("a+b+", RegexOptions.None); // Noncompliant
    }

    void Static(RegexOptions options)
    {
        var isMatch = Regex.IsMatch("some input", "a+b+"); // Noncompliant
        //            ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        var match = Regex.Match("some input", "a+b+"); // Noncompliant
        var matches = Regex.Matches("some input", "a+b+", RegexOptions.None); // Noncompliant
        var replace = Regex.Replace("some input", "a+b+", "some replacement", RegexOptions.None); // Noncompliant
        var split = Regex.Split("some input", "a+b+", options); // Noncompliant
    }
}

class ConstantPatterns
{
    const string Constant = "[0-9]";
    const string Quantified = "[0-9]+";
    static string mutable = "abc";

    void Safe(string input)
    {
        _ = new Regex("\\r\\n|\\r|\\n", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline); // Compliant (NET-4264)
        _ = new Regex("[0-9]");
        _ = new Regex("^abc$");
        _ = new Regex("(a|b)c");
        _ = new Regex("\\d\\d\\d");
        _ = new Regex(Constant);
        _ = Regex.IsMatch(input, "Model$");
    }

    void Risky(string input, string runtimePattern)
    {
        _ = new Regex("(?x) a b # comment");    // Noncompliant, '?' is not told apart from a quantifier
        _ = new Regex("(?:abc)");               // Noncompliant, same
        _ = new Regex("[0-9]+");                // Noncompliant, one quantifier is not covered yet
        _ = new Regex("\\+");                   // Noncompliant, escaped quantifier characters are not told apart
        _ = new Regex("[+*]");                  // Noncompliant, same inside a character class
        _ = new Regex("a{2}");                  // Noncompliant
        _ = new Regex("(a+)+");                 // Noncompliant
        _ = new Regex("(a|aa)+$");              // Noncompliant
        _ = new Regex("(?x)(a|aa) +$");         // Noncompliant
        _ = new Regex(runtimePattern);          // Noncompliant
        _ = new Regex(mutable);                 // Noncompliant, not a constant
        _ = new Regex(Quantified);              // Noncompliant, a constant with a quantifier
        _ = Regex.IsMatch(input, "[0-9]+");     // Noncompliant
    }

    void ParameterDefault(string withDefault = "abc")
    {
        _ = new Regex(withDefault);             // Noncompliant, a default value is not a constant
    }
}

class DoesNotCrash
{
    void MethodWithoutIdentifier(__arglist)
    {
        MethodWithoutIdentifier(__arglist(""));
    }
}

namespace NoSystem
{
    public class Regex
    {
        public static bool IsMatch(string input, string pattern)
        {
            return input == pattern;
        }
    }
}
