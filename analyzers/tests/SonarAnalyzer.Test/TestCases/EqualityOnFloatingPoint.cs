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

    void ZeroComparisons(float f, double d)
    {
        if (f == 0) { }               // Compliant zero is exactly representable
        if (f != 0) { }
        if (0 == f) { }
        if (0 != f) { }
        if (d == 0.0) { }
        if (d != 0.0) { }
        if (f == 0f) { }
        if (d == 0L) { }
        if (d == 0U) { }
        if (d == 0UL) { }
        if (d == -0.0) { }            // Compliant negative zero equals zero
        if (f == default(float)) { }  // Compliant
        if (d == default(double)) { } // Compliant

        const double Zero = 0.0;
        const double ZeroExpr = 0.0 + 0.0;
        if (d == Zero) { }
        if (d == ZeroExpr) { }

        double localZero = 0.0;
        const double NotZero = 0.1;
        if (d == localZero) { }       // Noncompliant
        if (d == NotZero) { }         // Noncompliant
    }

    // https://sonarsource.atlassian.net/browse/NET-3819
    void EqualsMethod(float f, double d1, double d2, decimal dec, int i, object o, string s)
    {
        if (d1.Equals(d2)) { }              // Noncompliant {{Do not check floating point equality with exact values, use a range instead.}}
        //     ^^^^^^
        if (f.Equals(3.14F)) { }            // Noncompliant {{Do not check floating point equality with exact values, use a range instead.}}
        _ = d1.Equals(1.2);                 // Noncompliant
        _ = ((double)i).Equals(d2);         // Noncompliant

        _ = d1.Equals(0.0);                 // Compliant zero is exactly representable
        _ = d1.Equals(0);                   // Compliant
        _ = d1.Equals(default(double));     // Compliant
        _ = d1.Equals(o);                   // Compliant Equals(object) overload, not a floating point comparison
        _ = d1.Equals(s);                   // Compliant Equals(object) overload
        _ = d1.Equals("x");                 // Compliant Equals(object) overload
        _ = i.Equals(3);                    // Compliant integer receiver
        _ = s.Equals("x");                  // Compliant string receiver
        _ = dec.Equals(1.2m);               // Compliant decimal is excluded
        _ = object.Equals(d1, d2);          // Compliant static object.Equals is out of scope
    }

    void EqualsMethod_Nullable(double? nd, float? nf, double d)
    {
        _ = nd?.Equals(d);              // Noncompliant {{Do not check floating point equality with exact values, use a range instead.}}
        //      ^^^^^^
        _ = nf?.Equals(3.14F);          // Noncompliant
        _ = nd?.Equals(d).ToString();   // Noncompliant {{Do not check floating point equality with exact values, use a range instead.}}
        _ = nd?.Equals(double.NaN);     // Noncompliant {{Do not check floating point equality with exact values, use 'double.IsNaN()' instead.}}
        _ = nd?.Equals(0.0);            // Compliant zero is exactly representable
        _ = nd.Value.Equals(d);         // Noncompliant - .Value is a double
        _ = nd.Equals(d);               // Compliant (FN): Nullable<double>.Equals(object) is not a floating point Equals overload
    }

    void EqualsMethod_SpecialMembers(double d, float f)
    {
        _ = d.Equals(double.NaN);               // Noncompliant {{Do not check floating point equality with exact values, use 'double.IsNaN()' instead.}}
        _ = double.NaN.Equals(d);               // Noncompliant {{Do not check floating point equality with exact values, use 'double.IsNaN()' instead.}}
        _ = d.Equals(double.PositiveInfinity);  // Noncompliant {{Do not check floating point equality with exact values, use 'double.IsPositiveInfinity()' instead.}}
        _ = d.Equals(double.NegativeInfinity);  // Noncompliant {{Do not check floating point equality with exact values, use 'double.IsNegativeInfinity()' instead.}}
        _ = f.Equals(float.NaN);                // Noncompliant {{Do not check floating point equality with exact values, use 'float.IsNaN()' instead.}}
        _ = f.Equals(float.PositiveInfinity);   // Noncompliant {{Do not check floating point equality with exact values, use 'float.IsPositiveInfinity()' instead.}}
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
