using System;

public class Base
{
    public Base() { }

    public Base(int x) { }

    public Base(string x) { }
}

public class A : Base
{
    public A(int x, int y) : base(x) { }

    private A(string x, string y) : base(x) { } // Method is empty

    public A(int i, int j, int z)
    {
        Console.WriteLine(j + z);
    }

    private A(string i, string j, string z) // Noncompliant
    {
        Console.WriteLine(j + z);
    }

    public A(int x, int y, string z) : this(x, y) { }

    private A(string x, string y, int z) : this(x, y) // Noncompliant
    {
        Console.WriteLine(x);
    }
}

public record class B
{
    public B(int i, int j)
    {
        Console.WriteLine(j);
    }

    private B(string i, int j) // Noncompliant
    {
        Console.WriteLine(j);
    }
}

public struct C
{
    public C(int i, int j)
    {
        Console.WriteLine(j);
    }

    private C(string i, int j) // Noncompliant
    {
        Console.WriteLine(j);
    }
}

public record struct D
{
    public D(int i, int j)
    {
        Console.WriteLine(j);
    }

    private D(string i, int j) // Noncompliant
    {
        Console.WriteLine(j);
    }
}
