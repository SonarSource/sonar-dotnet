using System.Text.RegularExpressions;

class Compliant
{
    void RawString()
    {
        var raw = new Regex("""
            [A
            B]
            """); // Compliant
    }
}

class Noncompliant
{
    void RawString()
    {
        var single = new Regex("""[A"""); // Noncompliant
        var multi = new Regex("""
            [A
            """); // Noncompliant @-2
    }
}
