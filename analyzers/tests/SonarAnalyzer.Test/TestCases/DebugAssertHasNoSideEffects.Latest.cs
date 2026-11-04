using System;
using System.Diagnostics;


var foo = new Foo();
Debug.Assert(foo.Put()); // Noncompliant
Debug.Assert(foo.Contains("a"));

record R
{
    R(Foo foo)
    {
        Debug.Assert(foo.Put()); // Noncompliant
        Debug.Assert(foo.Contains("a"));
    }

    Foo f;
    string Property
    {
        init
        {
            Debug.Assert(f.Put()); // Noncompliant
            Debug.Assert(f.Contains(value));
        }
    }
}

class Foo
{
    public bool Contains(string arg) => true;
    public bool Put() => true;
}

class ConditionalAssignment
{
    bool boolField;
    void Method(ConditionalAssignment x)
    {
        Debug.Assert((x?.boolField = false) ?? false);  // FN
    }
}

class CSharp15
{
    void PopCount(System.Collections.BitArray bits, int value, uint unsignedValue, System.Collections.Generic.Stack<int> stack)
    {
        Debug.Assert(bits.PopCount() == 0);                                         // Noncompliant FP NET-4913
        Debug.Assert(int.PopCount(value) == 1);                                     // Noncompliant FP NET-4913
        Debug.Assert(System.Numerics.BitOperations.PopCount(unsignedValue) == 1);   // Noncompliant FP NET-4913
        Debug.Assert(bits.Count == 8);
        Debug.Assert(stack.Pop() == 0);                                             // Noncompliant
    }
}
