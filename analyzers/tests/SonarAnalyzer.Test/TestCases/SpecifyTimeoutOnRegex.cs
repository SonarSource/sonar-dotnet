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
        var patternOnly = new Regex("some pattern"); // Noncompliant {{Pass a timeout to limit the execution time.}}
        //                ^^^^^^^^^^^^^^^^^^^^^^^^^
        var withOptions = new Regex("some pattern", RegexOptions.None); // Noncompliant
    }

    void Static(RegexOptions options)
    {
        var isMatch = Regex.IsMatch("some input", "some pattern"); // Noncompliant
        //            ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        var match = Regex.Match("some input", "some pattern"); // Noncompliant
        var matches = Regex.Matches("some input", "some pattern", RegexOptions.None); // Noncompliant
        var replace = Regex.Replace("some input", "some pattern", "some replacement", RegexOptions.None); // Noncompliant
        var split = Regex.Split("some input", "some pattern", options); // Noncompliant
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
