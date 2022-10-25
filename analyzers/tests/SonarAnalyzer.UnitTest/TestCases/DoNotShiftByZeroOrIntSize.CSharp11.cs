using System;

void UnsignedRightShiftOperator(int i, ulong q)
{
    i = i >>> 60; // FN
    i = i >>> 31; // Compliant

    q = q >>> 64; // FN
    q = q >>> 0; // Compliant
}
