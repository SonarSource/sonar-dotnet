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
        Regex patternOnly = new("A*"); // Noncompliant {{The regular expression should not match an empty string.}}
    }
}
