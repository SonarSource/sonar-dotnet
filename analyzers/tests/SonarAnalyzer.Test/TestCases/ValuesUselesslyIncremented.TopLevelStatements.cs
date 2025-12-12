using System;
using System.Numerics;

nint i = 0;
i = i++; // Noncompliant; i is still zero
i = ++i; // Compliant

nuint u = 0, a = 0;
u = u++; // Noncompliant; u is still zero
u = ++u; // Compliant
u += u++;
a = u++;

int x = (int) u++;

int Compute(nint v) =>
    (int) v++; // Noncompliant
//        ^^^

int ComputeArrowBody(int v) =>
    v--; // Noncompliant
//  ^^^

return (int)i++; // Noncompliant

IntPtr j = 0;
j = j++; // Noncompliant; i is still zero
j = ++j; // Compliant

UIntPtr v = 0, b = 0;
v = v++; // Noncompliant; u is still zero
v = ++v; // Compliant
v += v++; // Compliant
b = v++; // Compliant

int z = (int)u++;

IntPtr ComputeIntPtr(IntPtr v)
{
    return v++; // Noncompliant
//         ^^^
}

IntPtr ComputeArrowBodyIntPtr(IntPtr v) =>
    v--; // Noncompliant
//  ^^^
