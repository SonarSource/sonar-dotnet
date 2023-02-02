using System;
using System.Collections.Generic;
using System.Runtime.Versioning;

public class EqualityOnFloatingPoint
{
    void Test(float f, double d1, double d2, dynamic dyn)
    {
        if (dyn == 3.14) { }        // FN. {{Do not check floating point equality with exact values, use a range instead.}}
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
        double d = 1.0;

        if (d == double.NaN) { }      // Noncompliant {{Do not check floating point equality with exact values, use 'double.IsNaN()' instead.}}
        _ = d == double.NaN;          // Noncompliant {{Do not check floating point equality with exact values, use 'double.IsNaN()' instead.}}
        _ = double.NaN == d;          // Noncompliant {{Do not check floating point equality with exact values, use 'double.IsNaN()' instead.}}
        _ = double.NaN == double.NaN; // Noncompliant {{Do not check floating point equality with exact values, use 'double.IsNaN()' instead.}}
        _ = iPar == double.NaN;       // Noncompliant {{Do not check floating point equality with exact values, use 'double.IsNaN()' instead.}}
        _ = dPar == double.NaN;       // Noncompliant {{Do not check floating point equality with exact values, use 'double.IsNaN()' instead.}}
        _ = fPar == double.NaN;       // Noncompliant {{Do not check floating point equality with exact values, use 'double.IsNaN()' instead.}}
        _ = float.NaN == double.NaN;  // Noncompliant {{Do not check floating point equality with exact values, use 'double.IsNaN()' instead.}}
        _ = double.NaN == float.NaN;  // Noncompliant {{Do not check floating point equality with exact values, use 'double.IsNaN()' instead.}}
    }

    public void WithDoubleInequality(int iPar, double dPar, float fPar)
    {
        double d = 1.0;

        if (d != double.NaN) { }      // Noncompliant {{Do not check floating point inequality with exact values, use 'double.IsNaN()' instead.}}
        _ = d != double.NaN;          // Noncompliant {{Do not check floating point inequality with exact values, use 'double.IsNaN()' instead.}}
        _ = double.NaN != d;          // Noncompliant {{Do not check floating point inequality with exact values, use 'double.IsNaN()' instead.}}
        _ = double.NaN != double.NaN; // Noncompliant {{Do not check floating point inequality with exact values, use 'double.IsNaN()' instead.}}
        _ = iPar != double.NaN;       // Noncompliant {{Do not check floating point inequality with exact values, use 'double.IsNaN()' instead.}}
        _ = dPar != double.NaN;       // Noncompliant {{Do not check floating point inequality with exact values, use 'double.IsNaN()' instead.}}
        _ = fPar != double.NaN;       // Noncompliant {{Do not check floating point inequality with exact values, use 'double.IsNaN()' instead.}}
        _ = float.NaN != double.NaN;  // Noncompliant {{Do not check floating point inequality with exact values, use 'double.IsNaN()' instead.}}
        _ = double.NaN != float.NaN;  // Noncompliant {{Do not check floating point inequality with exact values, use 'double.IsNaN()' instead.}}
    }

    public void WithFloat(int iPar, float fPar)
    {
        float f = 1.0f;
        _ = f == float.NaN;    // Noncompliant {{Do not check floating point equality with exact values, use 'float.IsNaN()' instead.}}
        _ = f != float.NaN;    // Noncompliant {{Do not check floating point inequality with exact values, use 'float.IsNaN()' instead.}}
        _ = iPar == float.NaN; // Noncompliant {{Do not check floating point equality with exact values, use 'float.IsNaN()' instead.}}
        _ = fPar == float.NaN; // Noncompliant {{Do not check floating point equality with exact values, use 'float.IsNaN()' instead.}}
        _ = iPar != float.NaN; // Noncompliant {{Do not check floating point inequality with exact values, use 'float.IsNaN()' instead.}}
        _ = fPar != float.NaN; // Noncompliant {{Do not check floating point inequality with exact values, use 'float.IsNaN()' instead.}}
    }

    public void WithDoublePascalCase()
    {
        Double d = 1.0;
        _ = d == Double.NaN;        // Noncompliant {{Do not check floating point equality with exact values, use 'double.IsNaN()' instead.}}
        _ = d == System.Double.NaN; // Noncompliant {{Do not check floating point equality with exact values, use 'double.IsNaN()' instead.}}
        _ = d != Double.NaN;        // Noncompliant {{Do not check floating point inequality with exact values, use 'double.IsNaN()' instead.}}
        _ = d != System.Double.NaN; // Noncompliant {{Do not check floating point inequality with exact values, use 'double.IsNaN()' instead.}}
    }

    public void WithSingle()
    {
        Single f = 3.14159f;
        _ = f == Single.NaN;         // Noncompliant {{Do not check floating point equality with exact values, use 'float.IsNaN()' instead.}}
        _ = f == System.Single.NaN;  // Noncompliant {{Do not check floating point equality with exact values, use 'float.IsNaN()' instead.}}
        _ = f != Single.NaN;         // Noncompliant {{Do not check floating point inequality with exact values, use 'float.IsNaN()' instead.}}
        _ = f != System.Single.NaN;  // Noncompliant {{Do not check floating point inequality with exact values, use 'float.IsNaN()' instead.}}
    }
}

public class ReportSpecificMessage_Infinities
{
    public void WithDouble(double d)
    {
        _ = d == double.PositiveInfinity; // Noncompliant {{Do not check floating point equality with exact values, use 'double.IsPositiveInfinity()' instead.}}
        _ = double.PositiveInfinity != d; // Noncompliant {{Do not check floating point inequality with exact values, use 'double.IsPositiveInfinity()' instead.}}
        _ = d == double.NegativeInfinity; // Noncompliant {{Do not check floating point equality with exact values, use 'double.IsNegativeInfinity()' instead.}}
        _ = double.NegativeInfinity != d; // Noncompliant {{Do not check floating point inequality with exact values, use 'double.IsNegativeInfinity()' instead.}}
    }

    public void WithFloat(float f)
    {
        _ = f == float.PositiveInfinity; // Noncompliant {{Do not check floating point equality with exact values, use 'float.IsPositiveInfinity()' instead.}}
        _ = float.PositiveInfinity != f; // Noncompliant {{Do not check floating point inequality with exact values, use 'float.IsPositiveInfinity()' instead.}}
        _ = f == float.NegativeInfinity; // Noncompliant {{Do not check floating point equality with exact values, use 'float.IsNegativeInfinity()' instead.}}
        _ = float.NegativeInfinity != f; // Noncompliant {{Do not check floating point inequality with exact values, use 'float.IsNegativeInfinity()' instead.}}
    }

    public void WithDoublePascalCase(Double d)
    {
        _ = Double.PositiveInfinity == d;        // Noncompliant {{Do not check floating point equality with exact values, use 'double.IsPositiveInfinity()' instead.}}
        _ = d == System.Double.NegativeInfinity; // Noncompliant {{Do not check floating point equality with exact values, use 'double.IsNegativeInfinity()' instead.}}
    }

    public void WithSingle(Single f)
    {
        _ = Single.PositiveInfinity == f;        // Noncompliant {{Do not check floating point equality with exact values, use 'float.IsPositiveInfinity()' instead.}}
        _ = f == System.Single.NegativeInfinity; // Noncompliant {{Do not check floating point equality with exact values, use 'float.IsNegativeInfinity()' instead.}}
    }
}

namespace TestsWithTypeAliases
{
    using DoubleAlias = Double;

    public class ReportSpecificMessage
    {
        public void WithDoubleAlias()
        {
            DoubleAlias d = 1.674927471E-27;
            _ = d == DoubleAlias.NaN;     // Noncompliant {{Do not check floating point equality with exact values, use 'DoubleAlias.IsNaN()' instead.}}
            if (d == double.NaN) { }      // Noncompliant {{Do not check floating point equality with exact values, use 'DoubleAlias.IsNaN()' instead.}}
        }
    }
}

namespace TestWithUsingStatic
{
    using static System.Double;

    public class ReportSpecificMessage
    {
        public void WithUsingStatic(double d)
        {
            _ = d == NaN;          // Noncompliant {{Do not check floating point equality with exact values, use 'IsNaN()' instead.}}
            _ = NaN == d;          // Noncompliant {{Do not check floating point equality with exact values, use 'IsNaN()' instead.}}
            _ = NaN == NaN;        // Noncompliant {{Do not check floating point equality with exact values, use 'IsNaN()' instead.}}
            _ = NaN == float.NaN;  // Noncompliant {{Do not check floating point equality with exact values, use 'double.IsNaN()' instead.}}
            _ = double.NaN == NaN; // Noncompliant {{Do not check floating point equality with exact values, use 'double.IsNaN()' instead.}}
        }

        public void WithLocalVar(double d)
        {
            var NaN = 5;
            _ = d == NaN;   // Noncompliant {{Do not check floating point equality with exact values, use a range instead.}}
        }
    }
}
