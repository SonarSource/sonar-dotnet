using System;

int x = 2;

nint n = 2;
nuint nu = 2;

IntPtr intPtr = 2;
UIntPtr uintPtr = 2;

n.ToInt32(); // Compliant, FN, pure function
nu.ToUInt32(); // Compliant, FN, pure function

intPtr.ToInt64(); // Compliant, FN, pure function
uintPtr.ToUInt64(); // Compliant, FN, pure function

unsafe
{
    n.ToPointer(); // Compliant, FN, pure function
    intPtr.ToPointer(); // Compliant, FN, pure function


    nu.ToPointer(); // Compliant, FN, pure function
    uintPtr.ToPointer(); // Compliant, FN, pure function

    x.ToString(); // Noncompliant
}
