using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public class EqualityCheckOnModulus
    {
        public void isOdd(int x)
        {
            var y = x%2 == 1; // Noncompliant; if x is negative, x % 2 == -1
//                  ^^^^^^^^
            y = x%2 != -1; // Noncompliant {{The result of this modulus operation may not be negative.}}
            y = 1 == x%2; // Noncompliant {{The result of this modulus operation may not be positive.}}
            y = x%2 != 0;
            y = Math.Abs(x%2) == 1;

            var unsignedY = 54U;

            var xx = unsignedY % 4 == 1; //Compliant
        }
    }
}
