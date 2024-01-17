using System.Threading.Tasks;
using System;

internal interface IOperator<T> where T : IOperator<T>
{
    static virtual T operator ++(T input) => input; // Noncompliant {{Implement alternative method 'Increment' for the operator '++'.}}
    static abstract T operator --(T input); // Noncompliant
    static IOperator<T> operator ~(IOperator<T> input) => input; // Noncompliant
}
