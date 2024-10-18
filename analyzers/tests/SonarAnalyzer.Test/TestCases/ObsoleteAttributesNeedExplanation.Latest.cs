using System;
using System.Collections.Generic;
using System.Linq;

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

internal class TestCases
{
    public void Bar(IEnumerable<int> collection)
    {
        [Obsolete] int Get() => 1; // Noncompliant

        _ = collection.Select([Obsolete] (x) => x + 1); // Noncompliant

        Action a = [Obsolete] () => { }; // Noncompliant

        Action x = true
                       ? ([Obsolete] () => { }) // Noncompliant
                       : [Obsolete] () => { }; // Noncompliant

        Call([Obsolete("something")] (x) => { });
    }

    private void Call(Action<int> action) => action(1);
}

partial class PartialProperties
{
    [Obsolete] // Noncompliant
    partial int Value { get; set; }

    [Obsolete] // Noncompliant
    partial int this[int x] { get; set; }
}

partial class PartialProperties
{
    partial int Value { get => 42; set { } }

    partial int this[int x] { get => 42; set { } }
}

