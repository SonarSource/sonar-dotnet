using System.Diagnostics.CodeAnalysis;
using System;

[ExcludeFromCodeCoverage] // Noncompliant
void LocalMethod() { }

[ExcludeFromCodeCoverage] // Noncompliant
static void StaticLocalMethod() { }

[ExcludeFromCodeCoverage] // Noncompliant {{Add a justification.}}
public record Record
{
    public void Method()
    {
        [ExcludeFromCodeCoverage] // Noncompliant
        static void LocalMethod() { }
    }
}
