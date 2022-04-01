using System;

namespace Tests.Diagnostics
{
    class Program
    {
        void LoopTest(int x, int y)
        {
            for (nint i = 9; i >= 5;) { } // Compliant
            for (nuint i = 9; i >= 5;) { } // Compliant

            for (nint a = 0, b = 1; b < 1;) { } // Noncompliant
            for (nuint a = 0, b = 1; b < 1;) { } // Noncompliant
        }
    }
}
