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

    private int b;
    public int B() => b;

    private nint Value { get; init; }
    private nint UnusedValue { get; init; } // Noncompliant

    public Record Create() => new() { Value = 1 };

    private interface IFoo // Noncompliant
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

    private record UnusedNested1(string Name, int CategoryId);   // Noncompliant
    internal record UnusedNested2(string Name, int CategoryId);  // Noncompliant
    public record UnusedNested3(string Name, int CategoryId);

    private int usedInPatternMatching = 1;

    public int UseInPatternMatching(int val) =>
        val switch
        {
            < 0 => usedInPatternMatching,
            >= 0 => 1
        };

    private class LocalFunctionAttribute : Attribute { }

    public void Foo()
    {
        [LocalFunction]
        static void Bar()
        { }
    }
}

public class TargetTypedNew
{
    private TargetTypedNew(int arg)
    {
        var x = arg;
    }

    private TargetTypedNew(string arg) // Noncompliant - FP
    {
        var x = arg;
    }

    public static TargetTypedNew Create()
    {
        return new(42);
    }
}
