using System;
using System.Runtime.CompilerServices;

class Person
{
    const int forCoverage = 0;
    static int minAge = 0;
    static int Bar { get; set; } = 1;
    static event EventHandler Quix = (a, b) => { };

    static int maxAge = 42; // Noncompliant {{Remove the static member initializer, a static constructor or module initializer sets an initial value for the member.}}
    static string Name = "X"; // Noncompliant
    static string Job = "1"; // Noncompliant
    static bool Nice = true; // Noncompliant
    static int Foo { get; set; } = 1; // Noncompliant
    static event EventHandler Taz = (a, b) => { }; // Noncompliant

    public Person() { } // for coverage

    [ModuleInitializer]
    internal static void Initialize()
    {
        maxAge = 42;
    }

    [Obsolete]
    [ModuleInitializer]
    internal static void SecondInitialize()
    {
        Name = "Foo";
        Job = "2";
        Nice = false;
        Foo = 1;
        Taz = (a, b) => { };
    }

    [ModuleInitializer]
    internal static void ThirdInitialize()
    {
        Job = "3";
    }

    [Obsolete]
    internal static void Foo1()
    {
        minAge = 2;
    }

    [Obsolete("argument", false)]
    internal static void Foo2()
    {
        minAge = 2;
    }

    internal static void Foo3()
    {
        minAge = 3;
        Bar = 2;
        Quix = (a, b) => { };
    }
}

// we do not support records for instance members, see discussion in https://github.com/SonarSource/sonar-dotnet/pull/4756
record Foo
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

public record RecordWithParam(string bar)
{
    public string foo = "foo";
}

public record RecordWithParamAndConstructor(string bar)
{
    public string foo = "foo";
    public int quix = 1; // FN
    public RecordWithParamAndConstructor(string x, int y) : this(x)
    {
        quix = 2;
    }
}

public record StaticRecord
{
    public static int FOO = 1; // FN
    public static string BAR = "BAR";
    static StaticRecord()
    {
        FOO = 2;
    }
}

record EmptyRecordForCoverage { }

struct Struct1
{
    static int b = 1; // Noncompliant
    static int b2 = 2;

    [ModuleInitializer]
    internal static void Initialize() => b = 42;
}

interface SomeInterface
{
    static int Field = 1; // Noncompliant
    static int Field2 = 2;
    static event EventHandler Taz = (a, b) => { }; // Noncompliant

    [ModuleInitializer]
    internal static void Initialize()
    {
        Field = 2;
        Taz = (a, b) => { };
    }
}
