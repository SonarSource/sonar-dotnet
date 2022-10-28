using System;
using System.Numerics;

public class Noncompliant
{
    public WeirdOperatorOverload ThisExampleIsNoncompliant(WeirdOperatorOverload woo)
    {
        return woo--; // Noncompliant
    }
}

public class WeirdOperatorOverload : IDecrementOperators<WeirdOperatorOverload>
{
    public static WeirdOperatorOverload operator --(WeirdOperatorOverload value) => throw new NotImplementedException();
}

