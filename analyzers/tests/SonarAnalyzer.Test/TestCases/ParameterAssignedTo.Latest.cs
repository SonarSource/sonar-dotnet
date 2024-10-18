using System;

static void Foo(int x)
{
    Action<int, int> discard = (_, _) => _ = 10; // Compliant

    Action<int> underscoreName = _ => _ = 10; // Noncompliant

    Action<int, int> foo = (a, b) => a = 10; // Noncompliant
}

public record Record
{
    public int PropertyWithSet
    {
        set { value = 10; } // Noncompliant
    }

    public int PropertyWithInit
    {
        init { value = 10; } // Noncompliant
    }

    void Foo(int x)
    {
        x = 42; // Noncompliant
    }

    void Foo(nint x)
    {
        x += 42; // Compliant
    }
}

public record struct Record2
{
    void Foo(int x)
    {
        (int i, x) = (42, 42); // Noncompliant {{Introduce a new variable instead of reusing the parameter 'x'.}}
//              ^
    }

}

public class AClass
{
    void Foo(int x)
    {
        (int i, x) = (42, 42); // Noncompliant
    }
}

public partial class PartialProperties
{
    public partial int PartialProperty 
    {
        set { value = 10; } // Noncompliant
    }

    public partial int PartialProperty2
    {
        init { value = 10; } // Noncompliant
    }

    public partial int this[int x]
    {
        set { value = 10; } // Noncompliant
    }
}

public partial class PartialProperties
{
    public partial int PartialProperty { set; }
    public partial int PartialProperty2 { init; }
    public partial int this[int x] { set; }
}
