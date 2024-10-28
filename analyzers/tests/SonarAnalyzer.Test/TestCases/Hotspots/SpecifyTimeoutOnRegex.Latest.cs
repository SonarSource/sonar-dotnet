using System;
using System.Text.RegularExpressions;

class MyClass
{
    void ImplicitObject()
    {
        Regex patternOnly = new("some pattern"); // Noncompliant {{Pass a timeout to limit the execution time.}}
        //                  ^^^^^^^^^^^^^^^^^^^
        Regex withOptions = new("some pattern", RegexOptions.None); // Noncompliant
        Regex defaultOrder = new("some pattern", RegexOptions.None, TimeSpan.FromSeconds(1)); // Compliant
    }

    // Repro https://sonarsource.atlassian.net/browse/NET-227
    void EnumerateSplitsAndMatchesMethod(ReadOnlySpan<char> input, RegexOptions options)
    {
        foreach (Range r in Regex.EnumerateSplits(input, "something")) { }                                   // Noncompliant
        foreach (Range r in Regex.EnumerateSplits(input, "something", options)) { }                          // Noncompliant
        foreach (Range r in Regex.EnumerateSplits(input, "something", options, TimeSpan.FromSeconds(1))) { } // Compliant

        foreach (var m in Regex.EnumerateMatches(input, "something")) { }                                    // Noncompliant
        foreach (var m in Regex.EnumerateMatches(input, "something", options)) { }                           // Noncompliant
        foreach (var m in Regex.EnumerateMatches(input, "something", options, TimeSpan.FromSeconds(1))) { }  // Compliant
    }
}
