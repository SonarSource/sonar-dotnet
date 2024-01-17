using System;

public class A
{
    public A(int i, int j) // Noncompliant
    {
        Console.WriteLine(i);
        Console.WriteLine(j);
        Console.WriteLine(j);
    }
}

public record class B
{
    public B(int i, int j) // Noncompliant
    {
        Console.WriteLine(i);
        Console.WriteLine(j);
        Console.WriteLine(j);
    }
}

public struct C
{
    public C(int i, int j) // Noncompliant
    {
        Console.WriteLine(i);
        Console.WriteLine(j);
        Console.WriteLine(j);
    }
}

public record struct D
{
    public D(int i, int j) // Noncompliant
    {
        Console.WriteLine(i);
        Console.WriteLine(j);
        Console.WriteLine(j);
    }
}
