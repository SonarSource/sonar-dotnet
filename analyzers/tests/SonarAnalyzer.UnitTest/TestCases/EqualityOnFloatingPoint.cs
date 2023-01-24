using System;
using System.Collections.Generic;
using System.Runtime.Versioning;

public class EqualityOnFloatingPoint
{
    void Test(float f, double d1, double d2)
    {
        dynamic din = null;
        if (din == null) { }
        if (f == 3.14F) { }         // Noncompliant {{Do not check floating point equality with exact values, use a range instead.}}
        //    ^^
        if (f != 3.14F) { }         // Noncompliant {{Do not check floating point inequality with exact values, use a range instead.}}
        if (d1 == d2) { }           // Noncompliant
        if (d1 < 0 || d1 > 1) { }   // Compliant no indirect inequality test

        var b = d1 == 3.14;                   // Noncompliant
        if (true && f >= 3.146) { }           // Compliant no indirect equality test
        if (f <= 3.146 && ((f >= 3.146))) { } // Noncompliant indirect equality test
        if (3.146 >= f && 3.146 <= f) { }     // Noncompliant indirect equality test
        if (f <= 3.146 && 3.146 <= f) { }     // FN. Equivalent to the case above but not detected
        if (3.146 >= f && 3.146 < f) { }      // Compliant no indirect equality test
        if (f <= 3.146 && f > 3.146) { }      // Compliant no indirect equality test

        var i = 3;
        if (i <= 3 && i >= 3) { }             // Compliant: integer
        if (i < 4 || i > 4) { }               // Compliant: integer
        if (f < 3.146 || f > 3.146) { }       // Noncompliant indirect inequality test
        if (3.146 > f || 3.146 < f) { }       // Noncompliant indirect inequality test
        if (3.146 > f || f > 3.146) { }       // FN. Equivalent to the case above but not detected
        if (f < 3.146 || f >= 3.146) { }      // Compliant no indirect inequality
        if (3.146 > f || 3.146 <= f) { }      // Compliant no indirect inequality test

        if (f <= 3.146 && true && f >= 3.146) { } // Not recognized
        if (Math.Sign(f) == 0) { }                // Compliant

        float f1 = 0.0F;
        if ((System.Math.Sign(f1) == 0)) { }      // Compliant
    }
}

public class ReportSpecificMessage_NaN
{
    public void WithDoubleEquality(int iPar, double dPar, float fPar)
    {
        bool b;
        double d = 1.0;

        if (d == double.NaN) { }      // Noncompliant {{Do not check floating point equality with exact values, use 'double.IsNaN()' instead.}}
        b = d == double.NaN;          // Noncompliant {{Do not check floating point equality with exact values, use 'double.IsNaN()' instead.}}
        b = double.NaN == d;          // Noncompliant {{Do not check floating point equality with exact values, use 'double.IsNaN()' instead.}}
        b = double.NaN == double.NaN; // Noncompliant {{Do not check floating point equality with exact values, use 'double.IsNaN()' instead.}}
        b = iPar == double.NaN;       // Noncompliant {{Do not check floating point equality with exact values, use 'double.IsNaN()' instead.}}
        b = dPar == double.NaN;       // Noncompliant {{Do not check floating point equality with exact values, use 'double.IsNaN()' instead.}}
        b = fPar == double.NaN;       // Noncompliant {{Do not check floating point equality with exact values, use 'double.IsNaN()' instead.}}
        b = float.NaN == double.NaN;  // Noncompliant {{Do not check floating point equality with exact values, use 'double.IsNaN()' instead.}}

        // Wrong message: should be "use double.IsNaN()" instead
        b = double.NaN == float.NaN;  // Noncompliant {{Do not check floating point equality with exact values, use 'float.IsNaN()' instead.}}
    }

    public void WithDoubleInequality(int iPar, double dPar, float fPar)
    {
        bool b;
        double d = 1.0;

        if (d != double.NaN) { }      // Noncompliant {{Do not check floating point inequality with exact values, use 'double.IsNaN()' instead.}}
        b = d != double.NaN;          // Noncompliant {{Do not check floating point inequality with exact values, use 'double.IsNaN()' instead.}}
        b = double.NaN != d;          // Noncompliant {{Do not check floating point inequality with exact values, use 'double.IsNaN()' instead.}}
        b = double.NaN != double.NaN; // Noncompliant {{Do not check floating point inequality with exact values, use 'double.IsNaN()' instead.}}
        b = iPar != double.NaN;       // Noncompliant {{Do not check floating point inequality with exact values, use 'double.IsNaN()' instead.}}
        b = dPar != double.NaN;       // Noncompliant {{Do not check floating point inequality with exact values, use 'double.IsNaN()' instead.}}
        b = fPar != double.NaN;       // Noncompliant {{Do not check floating point inequality with exact values, use 'double.IsNaN()' instead.}}
        b = float.NaN != double.NaN;  // Noncompliant {{Do not check floating point inequality with exact values, use 'double.IsNaN()' instead.}}

        // Wrong message: should be "use double.IsNaN()" instead
        b = double.NaN != float.NaN;  // Noncompliant {{Do not check floating point inequality with exact values, use 'float.IsNaN()' instead.}}
    }

    public void WithFloat(int iPar, float fPar)
    {
        bool b;
        float f = 1.0f;

        b = f == float.NaN;    // Noncompliant {{Do not check floating point equality with exact values, use 'float.IsNaN()' instead.}}
        b = f != float.NaN;    // Noncompliant {{Do not check floating point inequality with exact values, use 'float.IsNaN()' instead.}}
        b = iPar == float.NaN; // Noncompliant {{Do not check floating point equality with exact values, use 'float.IsNaN()' instead.}}
        b = fPar == float.NaN; // Noncompliant {{Do not check floating point equality with exact values, use 'float.IsNaN()' instead.}}
        b = iPar != float.NaN; // Noncompliant {{Do not check floating point inequality with exact values, use 'float.IsNaN()' instead.}}
        b = fPar != float.NaN; // Noncompliant {{Do not check floating point inequality with exact values, use 'float.IsNaN()' instead.}}
    }

    public void WithDoublePascalCase()
    {
        bool b;
        Double d = 1.0;

        b = d == Double.NaN;        // Noncompliant {{Do not check floating point equality with exact values, use 'double.IsNaN()' instead.}}
        b = d == System.Double.NaN; // Noncompliant {{Do not check floating point equality with exact values, use 'double.IsNaN()' instead.}}
        b = d != Double.NaN;        // Noncompliant {{Do not check floating point inequality with exact values, use 'double.IsNaN()' instead.}}
        b = d != System.Double.NaN; // Noncompliant {{Do not check floating point inequality with exact values, use 'double.IsNaN()' instead.}}
    }

    public void WithSingle()
    {
        bool b;
        Single f = 3.14159f;

        b = f == Single.NaN;         // Noncompliant {{Do not check floating point equality with exact values, use 'float.IsNaN()' instead.}}
        b = f == System.Single.NaN;  // Noncompliant {{Do not check floating point equality with exact values, use 'float.IsNaN()' instead.}}
        b = f != Single.NaN;         // Noncompliant {{Do not check floating point inequality with exact values, use 'float.IsNaN()' instead.}}
        b = f != System.Single.NaN;  // Noncompliant {{Do not check floating point inequality with exact values, use 'float.IsNaN()' instead.}}
    }
}

namespace TestsWithTypeAliases
{
    using DoubleAlias = Double;

    public class ReportSpecificMessage_WithAliases
    {
        public void WithDoubleAlias()
        {
            bool b;
            DoubleAlias d1 = 1.674927471E-27;

            b = d1 == DoubleAlias.NaN;    // Noncompliant {{Do not check floating point equality with exact values, use 'DoubleAlias.IsNaN()' instead.}}

            double d2 = 1.674927471E-27;
            if (d2 == double.NaN) { }     // Noncompliant {{Do not check floating point equality with exact values, use 'DoubleAlias.IsNaN()' instead.}}
        }
    }
}
