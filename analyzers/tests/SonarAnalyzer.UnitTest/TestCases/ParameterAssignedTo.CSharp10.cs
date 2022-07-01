using System;

public record struct Record
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
