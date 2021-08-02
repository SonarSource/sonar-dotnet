using System;
using System.Runtime.CompilerServices;

class Person
{
    static int minAge = 0;
    static int maxAge = 42; // FN {{Remove the member initializer, module initializer sets an initial value for the member.}}
    static string Name = "X"; // FN
    static string Job = "1"; // FN
    static bool Nice = true; // FN

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
    }
}

record Foo
{
    static int foo = 1; // FN
    int bar = 42; // Noncompliant
    int fred { get; init; } = 42; // Noncompliant
    int quix = 9; // ok, not initialized in all constructors

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

