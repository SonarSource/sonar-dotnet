using System;
using System.Numerics;
using System.Runtime.InteropServices;

public class EqualityOnFloatingPoint
{
    bool HalfEqual(Half first, Half second)
        => first == second; // FN

    bool NFloatEqual(NFloat first, NFloat second)
        => first == second; // FN

    bool IsEpsilon<T>(T value) where T : IFloatingPointIeee754<T>
        => value == T.Epsilon; // FN

    bool AreEqual<T>(T first, T second) where T : IFloatingPointIeee754<T>
        => first == second; // FN
}
