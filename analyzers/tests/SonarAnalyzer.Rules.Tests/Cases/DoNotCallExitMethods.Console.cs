// version: CSharp9
using System;

Environment.Exit(0); // Noncompliant {{Remove this call to 'Environment.Exit' or ensure it is really required.}}

record R
{
    public void Foo() => Environment.Exit(0); // Noncompliant

    public string Prop
    {
        init => Environment.Exit(0); // Noncompliant
    }
}
