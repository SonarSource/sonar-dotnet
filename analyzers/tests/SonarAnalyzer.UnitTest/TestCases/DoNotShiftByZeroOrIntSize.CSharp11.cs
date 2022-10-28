using System;

class Program
{
    void UnsignedRightShiftOperator(int i, ulong q)
    {
        i = i >>> 60; // FN
        i = i >>> 31; // Compliant
        i >>>= 40; // FN

        q = q >>> 64; // FN
        q = q >>> 0; // Compliant
        q >>>= 70; // FN
    }
}
