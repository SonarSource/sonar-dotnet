using System.Text.RegularExpressions;

class Compliant
{
    private const string ValidPattern = "[A]";

    void ImplicitObject()
    {
        Regex defaultOrder = new("valid pattern"); // Compliant
    }

    void Interpolated(string subPattern)
    {
        var regex = new Regex($"{ValidPattern}"); //Compliant
        var combined = new Regex($"[AB{subPattern}"); // Compliant
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
        var regex = new Regex($"{InvalidPattern}"); // Noncompliant
        var combined = new Regex($"[AB{InvalidPattern}"); // Noncompliant
    }
}
