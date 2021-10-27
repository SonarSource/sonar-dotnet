using System;

public record struct Record
{
    void Foo(int x)
    {
        (int i, x) = (42, 42); // FN
    }

}

public class AClass
{
    void Foo(int x)
    {
        (int i, x) = (42, 42); // FN
    }

}
