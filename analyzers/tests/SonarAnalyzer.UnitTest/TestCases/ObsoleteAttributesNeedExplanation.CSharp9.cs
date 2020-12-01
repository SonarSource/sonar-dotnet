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
