using System;

int x = 2;

nint n = 2;
nuint nu = 2;

IntPtr intPtr = 2;
UIntPtr uintPtr = 2;

n.ToInt32(); // Noncompliant
nu.ToUInt32(); // Noncompliant

intPtr.ToInt64(); // Noncompliant
uintPtr.ToUInt64(); // Noncompliant

unsafe
{
    n.ToPointer(); // Noncompliant
    intPtr.ToPointer(); // Noncompliant


    nu.ToPointer(); // Noncompliant
    uintPtr.ToPointer(); // Noncompliant

    x.ToString(); // Noncompliant
}
