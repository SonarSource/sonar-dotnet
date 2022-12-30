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
        Regex patternOnly = new("[A"); // Noncompliant {{The pattern contains a syntax error: Invalid pattern '[A' at offset 2. Unterminated [] set.}}
    }
}
