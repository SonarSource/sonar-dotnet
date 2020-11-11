using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

void Empty() { } // FN

void WithComment()
{
    // because
}

void NotEmpty()
{
    Console.WriteLine();
}

record EmptyMethod
{
    void F2()
    {
        // Do nothing because of X and Y.
    }

    void F3()
    {
        Console.WriteLine();
    }

    [Conditional("DEBUG")]
    void F4()    // Noncompliant {{Add a nested comment explaining why this method is empty, throw a 'NotSupportedException' or complete the implementation.}}
    {
    }

    protected virtual void F5()
    {
    }

    extern void F6();

    [DllImport("avifil32.dll")]
    private static extern void F7();
}

abstract record MyR
{
    void F1() { } // Noncompliant
    public abstract void F2();
}

record MyR2 : MyR
{
    public override void F2()
    {
    }
}

class WithProp
{
    public string Prop
    {
        init { }
    }
}

class M
{
    [ModuleInitializer]
    internal static void M1() // Noncompliant
    {
    }

    [ModuleInitializer]
    internal static void M2()
    {
        // reason
    }

    [ModuleInitializer]
    internal static void M3()
    {
        Console.WriteLine();
    }
}

namespace D
{
    partial class C
    {
        public partial void Foo();
        public partial void Bar();
        public partial void Qix();
    }

    partial class C
    {
        public partial void Foo() { } // Noncompliant

        public partial void Bar()
        {
            // comment
        }

        public partial void Qix()
        {
            Console.WriteLine();
        }
    }
}
