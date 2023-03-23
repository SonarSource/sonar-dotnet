using System;

[Obsolete] // Noncompliant
void LocalMethod()
{ }

[Obsolete] // Noncompliant
static void StaticLocalMethod()
{ }

[Obsolete] // Noncompliant {{Add an explanation.}}
public record Record
{
    public void Method()
    {
        [Obsolete] // Noncompliant
        static void LocalMethod()
        { }
    }
}

class Noncompliant
{

    [Obsolete("", true, DiagnosticId = "42")] // Noncompliant
    void WithDiagnostics() { }

    [Obsolete("", true, DiagnosticId = "42", UrlFormat = "https://sonarsource.com")] // Noncompliant
    void WithDiagnosticsAndUrlFormat() { }
}

class Compliant
{
    [Obsolete("explanation", true, DiagnosticId = "42")]
    void WithDiagnostics() { }

    [Obsolete("explanation", true, DiagnosticId = "42", UrlFormat = "https://sonarsource.com")]
    void WithDiagnosticsAndUrlFormat() { }
}

class Ignore
{
    [Obsolete(UrlFormat = "https://sonarsource.com")]
    void NamedParametersOnly() { } // FN
}
