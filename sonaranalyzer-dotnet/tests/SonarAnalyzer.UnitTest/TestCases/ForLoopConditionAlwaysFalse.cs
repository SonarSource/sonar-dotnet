using System;

namespace Tests.Diagnostics
{
    class Program
    {
        void LoopTest(int x, int y)
        {
            for (var i = x; true;) { }
            for (var i = x; false;) { } // Noncompliant {{This loop will never execute.}}
            for (var i = x; !false;) { }
            for (var i = x; !!false;) { } // Noncompliant
            for (var i = x; true;) { }
            for (var i = x; !true;) { } // Noncompliant
            for (var i = x; ((!true));) { } // Noncompliant
            for (var i = x; (!(!true));) { }
            for (var i = x; !(!(!(true)));) { } // Noncompliant

            for (var i = 1; i < 5;) { }
            for (var i = 9; i < 5;) { } // Noncompliant
            for (var i = 9; i > 5;) { }
            for (var i = 1; i > 5;) { } // Noncompliant
            for (var i = 1; i <= 5;) { }
            for (var i = 9; i <= 5;) { } // Noncompliant
            for (var i = 9; i >= 5;) { }
            for (var i = 1; i >= 5;) { } // Noncompliant
            for (var i = 1; i == 1;) { }
            for (var i = 1; i == 2;) { } // Noncompliant
            for (var i = 1; i != 2;) { }
            for (var i = 1; i != 1;) { } // Noncompliant
            for (var i = 1; i != x;) { }
            for (var i = x; i < 5;) { }
            for (var i = 1; i < x;) { }
            for (var i = 1; i < -x;) { }

            for (int a = 0, b = 1; a >= 0;) { }
            for (int a = 0, b = 1; a >= 1;) { } // Noncompliant
            for (int a = 0, b = 1; b >= 1;) { }
            for (int a = 0, b = 1; b < 1;) { } // Noncompliant

            int c = 0;
            for (c = 1; c > 0;) { }
            for (c = 1; c > 2;) { } // Noncompliant

            for (var i = x; !(y == 1);) { }
            for (x++; y < 5;) { }
            for (int i; i < 5;) { }
            for (var i = 1; ;) { }
            for (var i = 0; i < 0x10;) { }
            for (var i = 0; i > 0x10;) { } // Noncompliant
            for (var i = 0; i < 0b10;) { }
            for (var i = 1; i <= 0Xffff; i++) { }
        }
    }
}
