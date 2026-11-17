using System;
using System.Text.RegularExpressions;

class MyClass
{
    void ImplicitObject()
    {
        Regex patternOnly = new("a+b+"); // Noncompliant {{Pass a timeout to limit the execution time.}}
        //                  ^^^^^^^^^^^
        Regex withOptions = new("a+b+", RegexOptions.None); // Noncompliant
        Regex defaultOrder = new("some pattern", RegexOptions.None, TimeSpan.FromSeconds(1)); // Compliant
    }

    // Repro https://sonarsource.atlassian.net/browse/NET-227
    void EnumerateSplitsAndMatchesMethod(ReadOnlySpan<char> input, RegexOptions options)
    {
        foreach (Range r in Regex.EnumerateSplits(input, "a+b+")) { }                                        // Noncompliant
        foreach (Range r in Regex.EnumerateSplits(input, "a+b+", options)) { }                               // Noncompliant
        foreach (Range r in Regex.EnumerateSplits(input, "something", options, TimeSpan.FromSeconds(1))) { } // Compliant

        foreach (var m in Regex.EnumerateMatches(input, "a+b+")) { }                                         // Noncompliant
        foreach (var m in Regex.EnumerateMatches(input, "a+b+", options)) { }                                // Noncompliant
        foreach (var m in Regex.EnumerateMatches(input, "something", options, TimeSpan.FromSeconds(1))) { }  // Compliant
    }
}

public static class Extensions
{
    extension(string s)
    {
        public Regex NoncompliantProp => new(s);                                              // Noncompliant
        public Regex CompliantProp => new(s, RegexOptions.None, TimeSpan.FromSeconds(1));     // Compliant

        public Regex NoncompliantMethod() => new(s);                                          // Noncompliant
        public Regex CompliantMethod() => new(s, RegexOptions.None, TimeSpan.FromSeconds(1)); // Compliant
    }
}

public class NulLConditionalAssignment
{
    public class Sample
    {
        public Regex Regex { get; set; }
    }

    public void Method(Sample sample)
    {
        sample?.Regex = new("a+b+");                                                  // Noncompliant
        sample?.Regex = new("something", RegexOptions.None, TimeSpan.FromSeconds(1)); // Compliant
    }
}
