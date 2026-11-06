using System;
using static System.GC;
GC.Collect(); // Noncompliant {{Refactor the code to remove this use of 'GC.Collect'.}}
GC.Collect(2, GCCollectionMode.Optimized); // Noncompliant
Collect(); // Noncompliant

record R
{
    void Foo() => GC.Collect(); // Noncompliant
    string Prop
    {
        init => GC.Collect(); // Noncompliant
    }
}

union IntOrBool(int, bool)
{
    void Foo() => GC.Collect(); // Noncompliant
}

closed class ClosedClass
{
    void Foo() => GC.Collect(); // Noncompliant
}
