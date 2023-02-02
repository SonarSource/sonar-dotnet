using System;
using System.Numerics;
using System.Runtime.InteropServices;

public class EqualityOnFloatingPoint
{
    bool HalfEqual(Half first, Half second) =>
        first == second;    // Noncompliant {{Do not check floating point equality with exact values, use a range instead.}}
    //        ^^

    bool NFloatEqual(NFloat first, NFloat second) =>
        first == second; // Noncompliant

    bool IsEpsilon<T>(T value) where T : IFloatingPointIeee754<T> =>
        value == T.Epsilon; // Noncompliant

    bool IsPi<T>(T value) where T : IFloatingPointIeee754<T> =>
        value <= T.Pi && ((value >= T.Pi)); // Noncompliant

    bool IsNotE<T>(T value) where T : IFloatingPointIeee754<T> =>
        value > T.E || ((value < T.E)); // Noncompliant

    bool AreEqual<T>(T first, T second) where T : IFloatingPointIeee754<T> =>
        first == second; // Noncompliant

    bool Equal<T>(T first, T second) where T : IBinaryFloatingPointIeee754<T> =>
        first == second; // Noncompliant
}

public class ReportSpecificMessage_NaN
{
    void HalfNaN(Half h)
    {
        _ = h == Half.NaN; // Noncompliant {{Do not check floating point equality with exact values, use 'Half.IsNaN()' instead.}}
        //    ^^
        h = Half.NaN;      // Compliant, not a comparison
    }

    void NFloatNaN(NFloat nf)
    {
        _ = nf == System.Runtime.InteropServices.NFloat.NaN; // Noncompliant {{Do not check floating point equality with exact values, use 'NFloat.IsNaN()' instead.}}
        _ = nf == NFloat.NaN;                                // Noncompliant {{Do not check floating point equality with exact values, use 'NFloat.IsNaN()' instead.}}
    }

    public void M<T>(T t) where T : IFloatingPointIeee754<T>
    {
        if (t == T.NaN) { } // Noncompliant {{Do not check floating point equality with exact values, use 'T.IsNaN()' instead.}}
        if (T.IsNaN(t)) { } // Compliant
    }
}

public class ReportSpecificMessage_Infinities
{
    void HalfInfinities(Half h)
    {
        _ = h == Half.PositiveInfinity; // Noncompliant {{Do not check floating point equality with exact values, use 'Half.IsPositiveInfinity()' instead.}}
        _ = Half.NegativeInfinity == h; // Noncompliant {{Do not check floating point equality with exact values, use 'Half.IsNegativeInfinity()' instead.}}
    }

    void NFloatInfinities(NFloat nf)
    {
        _ = nf == NFloat.PositiveInfinity; // Noncompliant {{Do not check floating point equality with exact values, use 'NFloat.IsPositiveInfinity()' instead.}}
        _ = NFloat.NegativeInfinity == nf; // Noncompliant {{Do not check floating point equality with exact values, use 'NFloat.IsNegativeInfinity()' instead.}}
    }
}

namespace TestWithUsingStatic
{
    using static System.Runtime.InteropServices.NFloat;

    public class ReportSpecificMessage
    {
        public void WithUsingStatic(double d)
        {
            _ = d == NaN;          // Noncompliant {{Do not check floating point equality with exact values, use 'IsNaN()' instead.}}
            _ = NaN == d;          // Noncompliant {{Do not check floating point equality with exact values, use 'IsNaN()' instead.}}
            _ = NaN == NaN;        // Noncompliant {{Do not check floating point equality with exact values, use 'IsNaN()' instead.}}
            _ = NaN == float.NaN;  // Noncompliant {{Do not check floating point equality with exact values, use 'NFloat.IsNaN()' instead.}}
            _ = double.NaN == NaN; // Noncompliant {{Do not check floating point equality with exact values, use 'double.IsNaN()' instead.}}
        }
    }
}
