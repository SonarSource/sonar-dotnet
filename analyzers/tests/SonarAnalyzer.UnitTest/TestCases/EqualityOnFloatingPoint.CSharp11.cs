using System;
using System.Numerics;
using System.Runtime.InteropServices;

public class EqualityOnFloatingPoint
{
    bool HalfEqual(Half first, Half second)
        => first == second;    // Noncompliant {{Do not check floating point equality with exact values, use a range instead.}}
    //           ^^

    bool NFloatEqual(NFloat first, NFloat second)
        => first == second;    // Noncompliant

    bool IsEpsilon<T>(T value) where T : IFloatingPointIeee754<T>
        => value == T.Epsilon; // Noncompliant

    bool AreEqual<T>(T first, T second) where T : IFloatingPointIeee754<T>
        => first == second;    // Noncompliant
}
