using System;

namespace Tests.Diagnostics
{
    public class Program
    {
        static readonly IntPtr intPtr1;     // Compliant
        static readonly UIntPtr uIntPtr1; // Compliant
        static readonly nint nint1; // Compliant
        static readonly nuint nuint1; // Compliant

        static readonly IntPtr intPtr2 = 42; // Compliant, FN, should be const
        static readonly UIntPtr uIntPtr2 = 42; // Compliant, FN, should be const
        static readonly nint nint2 = 42; // Compliant, FN, should be const
        static readonly nuint nuint2 = 42; // Compliant, FN, should be const

        static readonly bool x = true; // Noncompliant
    }
}

