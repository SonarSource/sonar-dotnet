using System;

int x = 2;

nint n = 2;
nuint nu = 2;

IntPtr intPtr = 2;
UIntPtr uintPtr = 2;

n.ToInt32(); // Compliant, FN, nint/IntPtr not considered an immutable type by the rule
nu.ToUInt32(); // Compliant, FN, nuint/UIntPtr not considered an immutable type by the rule

intPtr.ToInt64(); // Compliant, FN, IntPtr not considered an immutable type by the rule
uintPtr.ToUInt64(); // Compliant, FN, UIntPtr not considered an immutable type by the rule

unsafe
{
    n.ToPointer(); // Compliant, FN, nint/IntPtr not considered an immutable type by the rule
    intPtr.ToPointer(); // Compliant, FN, IntPtr not considered an immutable type by the rule


    nu.ToPointer(); // Compliant, FN, nuint/UIntPtr not considered an immutable type by the rule
    uintPtr.ToPointer(); // Compliant, FN, UIntPtr not considered an immutable type by the rule

    x.ToString(); // Noncompliant
}
