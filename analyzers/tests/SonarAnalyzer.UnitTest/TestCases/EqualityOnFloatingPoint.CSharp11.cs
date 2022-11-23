using System;
using System.Numerics;
using System.Runtime.InteropServices;

public class EqualityOnFloatingPoint
{
    bool HalfEqual(Half first, Half second)
        => first == second; // FN

    bool NFloatEqual(NFloat first, NFloat second)
        => first == second; // FN

    bool AreEqual<T>(IFloatingPointIeee754<T> first, IFloatingPointIeee754<T> second) where T : IFloatingPointIeee754<T>
        => first == second; // FN
}
