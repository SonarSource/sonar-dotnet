using System;

int a = 0;
int b = 1;
Console.WriteLine(b);

Product c = new("name", 1);
Product d = new("name", 2);
Console.WriteLine(d);

public record Product(string Name, int CategoryId);

public record Record
{
    private int a; // Noncompliant {{Remove the unused private field 'a'.}}
                   // Noncompliant@-1 - duplicate issue reported

    private int b;
    public int B() => b;

    private nint Value { get; init; }
    private nint UnusedValue { get; init; } // Noncompliant
                                            // Noncompliant@-1 - duplicate issue reported

    public Record Create() => new() { Value = 1 };

    private interface IFoo // Noncompliant
                           // Noncompliant@-1 - duplicate issue reported
    {
        public void Bar() { }
    }

    private record Nested(string Name, int CategoryId);

    public void UseNested()
    {
        Nested d = new("name", 2);
    }

    private record Nested2(string Name, int CategoryId);

    public void UseNested2()
    {
        _ = new Nested2("name", 2);
    }

    private int usedInPatternMatching = 1;

    public int UseInPatternMatching(int val) =>
        val switch
        {
            < 0 => usedInPatternMatching,
            >= 0 => 1
        };

    private class LocalFunctionAttribute : Attribute { } // Noncompliant - FP
                                                         // Noncompliant@-1 - duplicate issue reported

    public void Foo()
    {
        [LocalFunction]
        static void Bar()
        { }
    }
}

// See https://github.com/dotnet/roslyn/issues/45510
namespace System.Runtime.CompilerServices
{
    public class IsExternalInit { }
}
