// version: CSharp9
using System;
static void Foo(int x)
{
    Action<int, int> discard = (_, _) => _ = 10; // Compliant

    Action<int> underscoreName = _ => _ = 10; // Noncompliant

    Action<int, int> foo = (a, b) => a = 10; // Noncompliant
}

public record Record
{
    public int PropertyWithSet
    {
        set { value = 10; } // Noncompliant
    }

    public int PropertyWithInit
    {
        init { value = 10; } // Noncompliant
    }

    void Foo(int x)
    {
        x = 42; // Noncompliant
    }

    void Foo(nint x)
    {
        x += 42; // Compliant
    }
}
