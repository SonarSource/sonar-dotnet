using System;
using System.Numerics;
using System.Runtime.InteropServices;

public class EqualityOnFloatingPoint
{
    void TestHalfNaN()
    {
        bool b;
        var h = Half.NaN;

        b = h == Half.NaN; // Noncompliant {{Do not check floating point equality with exact values, use 'Half.IsNaN()' instead.}}
        //    ^^
    }

    void TestNFloatNaN()
    {
        bool b;
        var nf = System.Runtime.InteropServices.NFloat.NaN;

        b = nf == System.Runtime.InteropServices.NFloat.NaN; // Noncompliant {{Do not check floating point equality with exact values, use 'NFloat.IsNaN()' instead.}}
        b = nf == NFloat.NaN;                                // Noncompliant {{Do not check floating point equality with exact values, use 'NFloat.IsNaN()' instead.}}
    }

    bool HalfEqual(Half first, Half second)
        => first == second;    // Noncompliant {{Do not check floating point equality with exact values, use a range instead.}}
    //           ^^

    bool NFloatEqual(NFloat first, NFloat second)
        => first == second;    // Noncompliant

    bool IsEpsilon<T>(T value) where T : IFloatingPointIeee754<T>
        => value == T.Epsilon; // Noncompliant

    bool IsPi<T>(T value) where T : IFloatingPointIeee754<T>
        => value <= T.Pi && ((value >= T.Pi)); // Noncompliant

    bool IsNotE<T>(T value) where T : IFloatingPointIeee754<T>
        => value > T.E || ((value < T.E)); // Noncompliant

    bool AreEqual<T>(T first, T second) where T : IFloatingPointIeee754<T>
        => first == second;    // Noncompliant

    bool Equal<T>(T first, T second) where T : IBinaryFloatingPointIeee754<T>
        => first == second;    // Noncompliant
}
