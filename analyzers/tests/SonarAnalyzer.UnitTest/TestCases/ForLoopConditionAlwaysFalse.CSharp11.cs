using System;

namespace Tests.Diagnostics
{
    class Program
    {
        void LoopTest(int x, int y)
        {
            for (IntPtr i = 9; i >= 5;) { } // Compliant
            for (UIntPtr i = 9; i >= 5;) { } // Compliant

            for (IntPtr a = 0, b = 1; b < 1;) { } // Noncompliant
            for (UIntPtr a = 0, b = 1; b < 1;) { } // Noncompliant
        }
    }
}
