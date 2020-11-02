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

record C(string Foo, string Bar);

record Pair(C A, C B);

record Record
{
    void Method()
    {
        string unusedString = string.Empty; // Noncompliant
    }
}

// See https://github.com/dotnet/roslyn/issues/45510
namespace System.Runtime.CompilerServices
{
    public class IsExternalInit { }
}
