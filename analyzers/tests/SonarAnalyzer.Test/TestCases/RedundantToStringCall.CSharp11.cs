class Foo
{
    public void Method()
    {
        string rawStringLiteral = """Hello""";
        string magic = "Abracadabra";

        string interpolationWithNewLine = $"And for my next trick I'll do some {
            magic.ToString() // Noncompliant {{There's no need to call 'ToString()' on a string.}}
            }";

        var s = rawStringLiteral.ToString(); // Noncompliant {{There's no need to call 'ToString()' on a string.}}
        s = interpolationWithNewLine.ToString(); // Noncompliant {{There's no need to call 'ToString()' on a string.}}
    }
}
