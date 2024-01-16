using System.Diagnostics;

class PrimaryConstructor(string ctorParam = "http://foo.com") // Noncompliant
{
    void Method(string methodParam = "http://foo.com") // Noncompliant
    {
        var lambda = (string lambdaParam = "http://foo.com") => lambdaParam; // Noncompliant
    }
}

public class Telnet { }

public sealed class Repro
{
    public Telnet[] GetTelnet() => CreateTelnets();

    private static Telnet[] CreateTelnets() => [new()]; // Compliant - FN
}
