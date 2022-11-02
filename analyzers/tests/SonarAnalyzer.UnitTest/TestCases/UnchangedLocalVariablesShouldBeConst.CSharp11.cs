using System;

int i1 = 32; // Noncompliant 
const int i2 = 32; // Compliant

IntPtr foo = 42; // Noncompliant
UIntPtr foo2 = 42; // Noncompliant

const IntPtr constFoo = 42; // Compliant
const UIntPtr constFoo2 = 42; // Compliant

IntPtr bar = 31; // Compliant
bar++;

IntPtr bar2 = 31; // Compliant
bar2++;

IntPtr zero1 = IntPtr.Zero; // Compliant - IntPtr.Zero is not compile time constant
UIntPtr zero2 = UIntPtr.Zero; // Compliant - UIntPtr.Zero is not compile time constant

IntPtr compared = 42; // Noncompliant - (==) does not alter the value, should be const

if (compared == 42)
{
}

Func<nint> staticLambda1 = static () =>
{
    IntPtr v = 41; // Noncompliant
    return v;
};

Func<nuint> staticLamba2 = static () =>
{
    UIntPtr v = 41; // Noncompliant
    return v;
};

