using System;

string unusedString = string.Empty; // Noncompliant

C a = new("Foo", "1.0");
C b = new("Qux", "1.0");

Pair unusedPair = new(a, b); // Noncompliant

void ParentMethod() => Invoke(static (x, y) => x++);

void ParentMethod2() => Invoke(static (x, y) =>
{
    int z = 0; // Noncompliant
});

void Invoke(Action<int, int> action) => action(1, 2);

void DeclarationPattern()
{
    _ = new object() is object o; // Noncompliant
}

void PropertyPattern()
{
    _ = new object() is { } o; // Noncompliant
}

void PositionalPattern()
{
    _ = (1, 2) is (_, _) t; // Noncompliant
}

void VarPattern()
{
    _ = new object() is var o; // Noncompliant
}

void ParenthesizedDesignation()
{
    _ = (1, 2) is var (bar, _); // Noncompliant
}

record C(string Foo, string Bar);

record Pair(C A, C B);

record Record
{
    void Method()
    {
        string unusedString = string.Empty; // Noncompliant
    }
}
