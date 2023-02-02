using System.Text.RegularExpressions;

class Compliant
{
    void ImplicitObject()
    {
        Regex defaultOrder = new("some pattern"); // Compliant
    }
}

class Noncompliant
{
    void ImplicitObject()
    {
        Regex patternOnly = new("[A"); // Noncompliant {{Fix the syntax error inside this regex: Invalid pattern '[A' at offset 2. Unterminated [] set.}}
        Regex differentIssue = new("A???"); // Noncompliant {{Fix the syntax error inside this regex: Invalid pattern '[A' at offset 2. Unterminated [] set.}}
    }
}
