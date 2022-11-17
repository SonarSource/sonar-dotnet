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

            double number = 42;
            var greaterThan = number > double.NaN; // Noncompliant {{Do not compare a number with double.NaN.}}
            var greaterThan2 = double.NaN > number; // Noncompliant

            var greaterOrEqual = number >= double.NaN; // Noncompliant
            var greaterOrEqual2 = double.NaN >= number; // Noncompliant

            var lessThan = number < double.NaN; // Noncompliant
            var lessThan2 = double.NaN < number; // Noncompliant

            var lessOrEqualThan = number <= double.NaN; // Noncompliant
            var lessOrEqualThan2 = double.NaN <= number; // Noncompliant

            var isPattern = 42D is double.NaN; // Compliant, IsPattern is excluded, works as expected
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

            float number = 42;
            var greaterThan = number > float.NaN; // Noncompliant {{Do not compare a number with float.NaN.}}
            var greaterThan2 = float.NaN > number; // Noncompliant

            var greaterOrEqual = number >= float.NaN; // Noncompliant
            var greaterOrEqual2 = float.NaN >= number; // Noncompliant

            var lessThan = number < float.NaN; // Noncompliant
            var lessThan2 = float.NaN < number; // Noncompliant

            var lessOrEqualThan = number <= float.NaN; // Noncompliant
            var lessOrEqualThan2 = float.NaN <= number; // Noncompliant

            var isPattern = 42F is float.NaN; // Compliant, IsPattern is excluded, works as expected
        }
    }
}
