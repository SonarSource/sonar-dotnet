using System;
using System.Collections.Generic;
using System.Runtime.Versioning;

namespace Tests.Diagnostics
{
    using DoubleAlias = Double;

    public class EqualityOnFloatingPoint
    {
        void test(float f, double d1, double d2)
        {
            dynamic din = null;
            if (din == null)
            {
            }

            if (f == 3.14F) //Noncompliant {{Do not check floating point equality with exact values, use a range instead.}}
            //    ^^
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
            if (f < 3.146 || f >= 3.146) // Compliant no indirect inequality test
            {
            }
            if (3.146 > f || 3.146 <= f) // Compliant no indirect inequality test
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

        private class ReportSpecificMessage_NaN
        {

            public void WithDoubleEquality(int iPar1, double dPar1, float fPar1)
            {
                bool b1;
                double d1 = 1.0;

                // In a if statement
                if (d1 == double.NaN)          // Noncompliant {{Do not check floating point equality with exact values, use double.IsNaN() instead.}}
                { }

                // Special value in assigned boolean expression
                b1 = d1 == double.NaN;         // Noncompliant {{Do not check floating point equality with exact values, use double.IsNaN() instead.}}

                // Special value on the left side of the equality
                b1 = double.NaN == d1;         // Noncompliant {{Do not check floating point equality with exact values, use double.IsNaN() instead.}}

                // Special value on both sides of the equality
                b1 = double.NaN == double.NaN; // Noncompliant {{Do not check floating point equality with exact values, use double.IsNaN() instead.}}

                // With integer method parameter promotion
                b1 = iPar1 == double.NaN;      // Noncompliant {{Do not check floating point equality with exact values, use double.IsNaN() instead.}}

                // With double method parameter
                b1 = dPar1 == double.NaN;      // Noncompliant {{Do not check floating point equality with exact values, use double.IsNaN() instead.}}

                // With float method parameter promotion
                b1 = fPar1 == double.NaN;      // Noncompliant {{Do not check floating point equality with exact values, use double.IsNaN() instead.}}


                // With float NaN promotion, on right side
                // Wrong message: should be "use double.IsNaN()" instead
                b1 = double.NaN == float.NaN;  // Noncompliant {{Do not check floating point equality with exact values, use float.IsNaN() instead.}}

                // With float NaN promotion, on left side
                b1 = float.NaN == double.NaN;  // Noncompliant {{Do not check floating point equality with exact values, use double.IsNaN() instead.}}
            }

            public void WithDoubleInequality(int iPar1, double dPar1, float fPar1)
            {
                bool b1;
                double d1 = 1.0;

                if (d1 != double.NaN)          // Noncompliant {{Do not check floating point inequality with exact values, use double.IsNaN() instead.}}
                { }

                b1 = d1 != double.NaN;         // Noncompliant {{Do not check floating point inequality with exact values, use double.IsNaN() instead.}}
                b1 = double.NaN != d1;         // Noncompliant {{Do not check floating point inequality with exact values, use double.IsNaN() instead.}}
                b1 = double.NaN != double.NaN; // Noncompliant {{Do not check floating point inequality with exact values, use double.IsNaN() instead.}}

                b1 = iPar1 != double.NaN;      // Noncompliant {{Do not check floating point inequality with exact values, use double.IsNaN() instead.}}
                b1 = dPar1 != double.NaN;      // Noncompliant {{Do not check floating point inequality with exact values, use double.IsNaN() instead.}}
                b1 = fPar1 != double.NaN;      // Noncompliant {{Do not check floating point inequality with exact values, use double.IsNaN() instead.}}

                b1 = double.NaN != float.NaN;  // Noncompliant {{Do not check floating point inequality with exact values, use float.IsNaN() instead.}}
                b1 = float.NaN != double.NaN;  // Noncompliant {{Do not check floating point inequality with exact values, use double.IsNaN() instead.}}
            }

            public void WithFloat(int iPar1, float fPar1)
            {
                bool b1;
                float f1 = 1.0f;

                b1 = f1 == float.NaN;    // Noncompliant {{Do not check floating point equality with exact values, use float.IsNaN() instead.}}
                b1 = f1 != float.NaN;    // Noncompliant {{Do not check floating point inequality with exact values, use float.IsNaN() instead.}}

                b1 = iPar1 == float.NaN; // Noncompliant {{Do not check floating point equality with exact values, use float.IsNaN() instead.}}
                b1 = fPar1 == float.NaN; // Noncompliant {{Do not check floating point equality with exact values, use float.IsNaN() instead.}}
                b1 = iPar1 != float.NaN; // Noncompliant {{Do not check floating point inequality with exact values, use float.IsNaN() instead.}}
                b1 = fPar1 != float.NaN; // Noncompliant {{Do not check floating point inequality with exact values, use float.IsNaN() instead.}}
            }

            public void WithDoublePascalCase()
            {
                bool b1;
                Double d1 = 1.0;

                b1 = d1 == Double.NaN;         // Noncompliant {{Do not check floating point equality with exact values, use double.IsNaN() instead.}}
                b1 = d1 == System.Double.NaN;  // Noncompliant {{Do not check floating point equality with exact values, use double.IsNaN() instead.}}
                b1 = d1 != Double.NaN;         // Noncompliant {{Do not check floating point inequality with exact values, use double.IsNaN() instead.}}
                b1 = d1 != System.Double.NaN;  // Noncompliant {{Do not check floating point inequality with exact values, use double.IsNaN() instead.}}
            }

            public void WithSingle()
            {
                bool b1;
                Single f1 = 3.14159f;

                b1 = f1 == Single.NaN;         // Noncompliant {{Do not check floating point equality with exact values, use float.IsNaN() instead.}}
                b1 = f1 == System.Single.NaN;  // Noncompliant {{Do not check floating point equality with exact values, use float.IsNaN() instead.}}
                b1 = f1 != Single.NaN;         // Noncompliant {{Do not check floating point inequality with exact values, use float.IsNaN() instead.}}
                b1 = f1 != System.Single.NaN;  // Noncompliant {{Do not check floating point inequality with exact values, use float.IsNaN() instead.}}
            }

            public void WithDoubleAlias()
            {
                bool b1;
                DoubleAlias d1 = 1.674927471E-27;

                b1 = d1 == DoubleAlias.NaN;    // Noncompliant {{Do not check floating point equality with exact values, use double.IsNaN() instead.}}
            }
        }
    }
}
