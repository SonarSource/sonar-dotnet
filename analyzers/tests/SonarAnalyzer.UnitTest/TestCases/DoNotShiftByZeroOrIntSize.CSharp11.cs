using System;

class Program
{
    void UnsignedRightShiftOperator(int i, ulong q)
    {
        i = i >>> 60; // Noncompliant
        i = i >>> 31; // Compliant
        i >>>= 40; // Noncompliant

        q = q >>> 64; // Noncompliant
        q = q >>> 0; // Compliant
        q >>>= 70; // Noncompliant
    }
}
