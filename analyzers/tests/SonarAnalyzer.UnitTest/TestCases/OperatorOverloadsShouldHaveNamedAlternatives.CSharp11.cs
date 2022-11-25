using System.Threading.Tasks;
using System;

internal interface IOperator<T> where T : IOperator<T>
{
    static virtual T operator ++(T input) => input; // Noncompliant {{Implement alternative method 'Increment' for the operator '++'.}}
}

public class Foo : IOperator<Foo>
{
    public static Foo operator --(Foo a) => a; // Noncompliant
}


