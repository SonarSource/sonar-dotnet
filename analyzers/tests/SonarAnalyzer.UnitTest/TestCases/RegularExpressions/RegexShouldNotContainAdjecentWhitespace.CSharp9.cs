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
        Regex patternOnly = new("A  B"); // Noncompliant {{The pattern contains adjacent whitespace.}}
    }
}
