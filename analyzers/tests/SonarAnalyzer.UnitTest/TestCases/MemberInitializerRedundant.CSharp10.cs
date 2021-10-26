using System;
using System.Runtime.CompilerServices;

// we do not support records for instance members, see discussion in https://github.com/SonarSource/sonar-dotnet/pull/4756
record struct Foo
{
    static int foo = 1; // FN
    int bar = 42; // FN
    int fred { get; init; } = 42; // FN
    int quix = 9; // ok, not initialized in all constructors
    const int barney = 1;

    [ModuleInitializer]
    internal static void Initialize()
    {
        foo = 1;
    }

    public Foo()
    {
        bar = 30;
        quix = 10;
        fred = 10;
    }

    public Foo(int x)
    {
        bar = x;
        fred = 10;
    }

    public Foo(int x, int y) : this(x) { }
}

public record struct RecordWithParam(string bar)
{
    public string foo = "foo";
}

public record struct RecordWithParamAndConstructor(string bar)
{
    public string foo = "foo";
    public int quix = 1; // FN
    public RecordWithParamAndConstructor(string x, int y) : this(x)
    {
        quix = 2;
    }
}

public record struct StaticRecord
{
    public static int FOO = 1; // FN
    public static string BAR = "BAR";
    static StaticRecord()
    {
        FOO = 2;
    }
}

record struct EmptyRecordForCoverage { }

struct Struct1
{
    static int b = 1; // Noncompliant
    static int b2 = 2;

    [ModuleInitializer]
    internal static void Initialize() => b = 42;
}
