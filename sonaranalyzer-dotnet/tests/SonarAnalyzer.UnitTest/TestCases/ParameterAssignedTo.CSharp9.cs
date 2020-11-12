using System;

static void Foo(int x)
{
    Action<int, int> discard = (_, _) => _ = 10; // Compliant

    Action<int, int> foo = (a, b) => a = 10; // Noncompliant
}

public record Record
{
    void Foo(int x)
    {
        x = 42; // Noncompliant
    }

    void Foo(nint x)
    {
        x += 42; // Compliant
    }
}
