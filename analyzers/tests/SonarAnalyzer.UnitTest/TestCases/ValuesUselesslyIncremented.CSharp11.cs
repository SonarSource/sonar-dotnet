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

public class NativeIntTestcases
{
    public nint Method()
    {
        IntPtr i = 0;
        i = i++; // Noncompliant; i is still zero
        i = ++i; // Compliant

        UIntPtr u = 0, a = 0;
        u = u++; // Noncompliant; u is still zero
        u = ++u; // Compliant
        u += u++;
        a = u++;

        int x = (int)u++;

        return i++; // Noncompliant
    }

    IntPtr Compute(IntPtr v)
    {
        return v++; // Noncompliant
//             ^^^
    }

    IntPtr ComputeArrowBody(IntPtr v) =>
        v--; // Noncompliant
//      ^^^

}
