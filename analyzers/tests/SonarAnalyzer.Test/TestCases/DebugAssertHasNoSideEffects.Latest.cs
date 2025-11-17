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
