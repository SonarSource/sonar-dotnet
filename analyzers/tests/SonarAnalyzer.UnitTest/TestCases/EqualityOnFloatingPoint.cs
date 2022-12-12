using System;
using System.Collections.Generic;
using System.Runtime.Versioning;

namespace Tests.Diagnostics
{
    public class EqualityOnFloatingPoint
    {
        void test(float f, double d1, double d2)
        {
            dynamic din = null;
            if (din == null)
            {
            }

            if (f == 3.14F) //Noncompliant {{Do not check floating point equality with exact values, use a range instead.}}
//                ^^
            {
            }

            if (f != 3.14F) //Noncompliant {{Do not check floating point inequality with exact values, use a range instead.}}
            { }

            if (d1 == d2) //Noncompliant
            { }

            var b = d1 == 3.14; //Noncompliant

            if (true && f >= 3.146)
            {
            }

            if (f <= 3.146 && ((f >= 3.146))) // Noncompliant indirect equality test
            {
            }
            if (3.146 >= f && 3.146 <= f) // Noncompliant indirect equality test
            {
            }
            if (f <= 3.146 && 3.146 <= f) // FN. Equivalent to the case above but not detected
            {
            }
            if (3.146 >= f && 3.146 < f)  // Compliant no indirect equality test
            {
            }
            if (f <= 3.146 && f > 3.146)  // Compliant no indirect equality test
            {
            }
            var i = 3;
            if (i <= 3 && i >= 3)
            {
            }

            if (i < 4 || i > 4)
            {
            }

            if (f < 3.146 || f > 3.146) // Noncompliant indirect inequality test
            {
            }
            if (3.146 > f || 3.146 < f) // Noncompliant indirect inequality test
            {
            }
            if (3.146 > f || f > 3.146) // FN. Equivalent to the case above but not detected
            {
            }

            if (f <= 3.146 && true && f >= 3.146) // Not recognized
            {
            }

            if (Math.Sign(f) == 0)
            {
            }

            float f1 = 0.0F;
            if ((System.Math.Sign(f1) == 0))
            {
            }
        }
    }
}
