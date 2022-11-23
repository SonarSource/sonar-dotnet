using System.Threading.Tasks;
using System;

internal interface IOperator<T> where T : IOperator<T>
{
    static virtual T operator ++(T input) => input; // Noncompliant {{Implement alternative method 'Increment' for the operator '++'.}}

    static virtual T Increase(T input) => input;
}
