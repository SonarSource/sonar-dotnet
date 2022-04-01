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

            var z = 0;
            for (int i = 0; i < z; i++) { } // FN - we only check for literals in the condition
            for (; z < 0; z++) { } // FN - we only check for initializers inside the loop statement

            // Reproducer for https://github.com/SonarSource/sonar-dotnet/issues/5428
            for (float n = 0.0F; n > -0.1F; n -= 0.005F) { } // Compliant
            for (double n = 0.0; n > -0.1; n -= 0.005) { } // Compliant
            for (var n = 0.0; n > -0.1; n -= 0.005) { } // Compliant
            for (decimal n = 0.0M; n > -0.1M; n -= 0.005M) { } // Compliant

            for (float n = -0.16F; n == -0.23F;) { } // Noncompliant
            for (double n = -0.42; n != -0.42;) { } // Noncompliant
            for (var n = 0.0; n <= -0.1; n -= 0.005) { } // Noncompliant
            for (decimal n = 0.0M; n <= -0.1M; n -= 0.005M) { } // Noncompliant

            for (var i = 9; i < 4 - 2;) { } // FN
        }
    }
}
