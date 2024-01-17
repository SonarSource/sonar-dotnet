using System;

Environment.Exit(0); // Compliant

Action asd = () => Environment.Exit(0); // Noncompliant

void LocalFunction()
{
    Environment.Exit(0); // Noncompliant
}

record R
{
    public void Foo() => Environment.Exit(0); // Noncompliant

    public string Prop
    {
        init => Environment.Exit(0); // Noncompliant
    }
}
