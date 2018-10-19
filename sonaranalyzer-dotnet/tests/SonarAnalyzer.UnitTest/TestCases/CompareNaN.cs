using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    struct Point { public float X; public float Y; }
    class CompareNaN
    {
        void TestDouble()
        {
            var a = double.NaN;

            if (a == double.NaN) // Noncompliant {{Use double.IsNaN() instead.}}
            {
                Console.WriteLine("a is not a number");
            }

            if (a != double.NaN) // Noncompliant {{Use double.IsNaN() instead.}}
            {
                Console.WriteLine("a is a number");
            }
        }

        void TestFloat()
        {
            var a = float.NaN;

            if (a == float.NaN) // Noncompliant {{Use float.IsNaN() instead.}}
            {
                Console.WriteLine("a is not a number");
            }

            if (a != float.NaN) // Noncompliant {{Use float.IsNaN() instead.}}
            {
                Console.WriteLine("a is a number");
            }

            if (a != a) // Compliant
            {
                Console.WriteLine("a is not a number");
            }

            int b = 5;
            if (new Point().X != b) // Compliant
            {
                Console.WriteLine("this is ok");
            }
        }
    }
}
