using System;
using System.Runtime.CompilerServices;

namespace Tests.CustomType
{
    public class ModuleInitializerAttribute : Attribute { }

    public class Foo
    {
        [ModuleInitializer]
        internal static void M1()
        {
            throw new Exception(); // Compliant - the attribute is not in the System.Runtime.CompilerServices namespace
        }
    }
}

record Record1 : IDisposable
{
    static Record1() => throw new NotImplementedException(); // Compliant (allowed exception)
    public void Dispose()
    {
        throw new NotImplementedException(); // Compliant (allowed exception)
    }
}

record Record2 : IDisposable
{
    static Record2() => throw new Exception(); // Noncompliant {{Remove this 'throw' expression.}}
//                      ^^^^^^^^^^^^^^^^^^^^^

    [ModuleInitializer]
    internal static void M1()
    {
        throw new Exception(); // Noncompliant
    }

    [Obsolete]
    [System.Runtime.CompilerServices.ModuleInitializerAttribute]
    internal static void M2() => throw new Exception(); // Noncompliant

    [Obsolete]
    internal static void M3() => throw new Exception(); // Compliant - different attribute

    [Obsolete("This attribute has arguments.", true)]
    internal static void M4() => throw new Exception(); // Compliant - different attribute

    event EventHandler OnSomething
    {
        add
        {
            throw new Exception(); // Noncompliant
        }
        remove
        {
            throw new InvalidOperationException(); // Compliant - allowed exception
        }
    }

    event EventHandler OnSomeArrow
    {
        add => throw new Exception(); // Noncompliant
        remove => throw new InvalidOperationException(); // Compliant - allowed exception
    }

    public override string ToString()
    {
        throw new Exception(); // Noncompliant
    }

    public void Dispose()
    {
        throw new Exception(); // Noncompliant
    }
}

interface IMyInterface
{
    static virtual event EventHandler VirtualEvent
    {
        add => throw new Exception(); // Noncompliant
        remove => throw new InvalidOperationException(); // Compliant - allowed exception
    }

    static abstract event EventHandler AbstractEvent;
}

class MyClass : IMyInterface
{
    public static event EventHandler AbstractEvent
    {
        add => throw new Exception(); // Noncompliant
        remove => throw new InvalidOperationException(); // Compliant - allowed exception
    }
}

partial class PartialClass
{
    partial PartialClass() => throw new Exception();    // Compliant

    partial event EventHandler PartialEvent
    {
        add => throw new Exception();                   // Noncompliant
        remove { throw new Exception(); }               // Noncompliant
    }

    static partial event EventHandler StaticPartialEvent
    {
        add { throw new Exception(); }                  // Noncompliant
        remove => throw new Exception();                // Noncompliant
    }
}

namespace Extensions
{
    class Sample { }

    static class Extensions
    {
        extension(Sample)
        {
            public static bool operator ==(Sample a, Sample b)
            {
                throw new Exception(); // Noncompliant
            }

            public static bool operator !=(Sample a, Sample b)
            {
                throw new Exception(); // Noncompliant
            }
        }

        extension(Sample s)
        {
            public bool Equals(object obj) => throw new Exception();    // Noncompliant FP NET-2707
            public int GetHashCode() => throw new Exception();          // Compliant
            public string ToString() { throw new Exception(); }         // Compliant
            public void Dispose() { throw new Exception(); }            // Compliant
        }
    }
}
