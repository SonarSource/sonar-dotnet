using System;
using System.Numerics;

IntPtr i = 0;
i = i++; // Noncompliant; i is still zero
i = ++i; // Compliant

UIntPtr u = 0, a = 0;
u = u++; // Noncompliant; u is still zero
u = ++u; // Compliant
u += u++; // Compliant
a = u++; // Compliant

int x = (int)u++;

IntPtr Compute(IntPtr v)
{
    return v++; // Noncompliant
//         ^^^
}

IntPtr ComputeArrowBody(IntPtr v) =>
    v--; // Noncompliant
//  ^^^

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
