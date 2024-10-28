using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

class Compliant
{
    private const string ValidPattern = "[A]";

    void ImplicitObject()
    {
        Regex defaultOrder = new("valid pattern");    // Compliant
    }

    void Interpolated(string subPattern)
    {
        var regex = new Regex($"{ValidPattern}");     // Compliant
        var combined = new Regex($"[AB{subPattern}"); // Compliant
    }

    void RawString()
    {
        var raw = new Regex("""
            [A
            B]
            """); // Compliant
    }

    void DiagnosticStringSyntaxAttribute()
    {
        MyMethod("[0-9]+"); // Compliant

        void MyMethod([StringSyntax(StringSyntaxAttribute.Regex)] string regex) { }
    }

    void NewEnumerateSplitsMethod(ReadOnlySpan<char> input, RegexOptions options)
    {
        foreach (Range r in Regex.EnumerateSplits(input, "[0-9]+")) { }                                   // Compliant
        foreach (Range r in Regex.EnumerateSplits(input, "[0-9]+", options)) { }                          // Compliant
        foreach (Range r in Regex.EnumerateSplits(input, "[0-9]+", options, TimeSpan.FromSeconds(1))) { } // Compliant

        foreach (var m in Regex.EnumerateMatches(input, "[A-Z]")) { }                                     // Compliant
        foreach (var m in Regex.EnumerateMatches(input, "[A-Z]", options)) { }                            // Compliant
        foreach (var m in Regex.EnumerateMatches(input, "[A-Z]", options, TimeSpan.FromSeconds(1))) { }   // Compliant
    }
}

class Noncompliant
{
    private const string InvalidPattern = "[A";

    void ImplicitObject()
    {
        Regex patternOnly = new("[A");      // Noncompliant {{Fix the syntax error inside this regex: Invalid pattern '[A' at offset 2. Unterminated [] set.}}
        Regex differentIssue = new("A???"); // Noncompliant {{Fix the syntax error inside this regex: Invalid pattern 'A???' at offset 4. Nested quantifier '?'.}}
    }

    void Interpolated()
    {
        var regex = new Regex($"{InvalidPattern}");       // Noncompliant
        var combined = new Regex($"[AB{InvalidPattern}"); // Noncompliant
    }

    void RawString()
    {
        var single = new Regex("""[A"""); // Noncompliant
        var multi = new Regex("""
            [A
            """); // Noncompliant @-2
    }

    // Repro https://sonarsource.atlassian.net/browse/NET-236
    void DiagnosticStringSyntaxAttribute()
    {
        MyMethod("[A"); // FN

        void MyMethod([StringSyntax(StringSyntaxAttribute.Regex)] string regex) { }
    }

    // Repro https://sonarsource.atlassian.net/browse/NET-228
    void EnumerateSplitsAndMatchesMethod(ReadOnlySpan<char> input, RegexOptions options)
    {
        foreach (Range r in Regex.EnumerateSplits(input, "[A")) { }                                   // Noncompliant
        foreach (Range r in Regex.EnumerateSplits(input, "[A", options)) { }                          // Noncompliant
        foreach (Range r in Regex.EnumerateSplits(input, "[A", options, TimeSpan.FromSeconds(1))) { } // Noncompliant

        foreach (var m in Regex.EnumerateMatches(input, "[A")) { }                                    // Noncompliant
        foreach (var m in Regex.EnumerateMatches(input, "[A", options)) { }                           // Noncompliant
        foreach (var m in Regex.EnumerateMatches(input, "[A", options, TimeSpan.FromSeconds(1))) { }  // Noncompliant
    }

    void EscapeSequence()
    {
        var regex = new Regex("\e[a|b]"); // Compliant
        regex = new Regex("[a|\eb]");     // Compliant
        regex = new Regex("[a|\u001bb]"); // Compliant
        regex = new Regex("[a|b]\e");     // Compliant
    }
}
