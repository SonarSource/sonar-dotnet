using System;

Environment.Exit(0); // Compliant

record R
{
    public void Foo() => Environment.Exit(0); // Noncompliant

    public string Prop
    {
        init => Environment.Exit(0); // Noncompliant
    }
}
