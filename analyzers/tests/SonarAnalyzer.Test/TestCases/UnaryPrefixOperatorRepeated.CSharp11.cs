using System;
using System.Numerics;

public class Noncompliant
{
    public void NoncompliantExample(WeirdOperatorOverride woo)
    {
        var result = ~~woo; // Noncompliant
    }
}

public class WeirdOperatorOverride : IBitwiseOperators<WeirdOperatorOverride, WeirdOperatorOverride, WeirdOperatorOverride>
{
    public static WeirdOperatorOverride operator ~(WeirdOperatorOverride value) => throw new NotImplementedException();
    public static WeirdOperatorOverride operator &(WeirdOperatorOverride left, WeirdOperatorOverride right) => throw new NotImplementedException();
    public static WeirdOperatorOverride operator |(WeirdOperatorOverride left, WeirdOperatorOverride right) => throw new NotImplementedException();
    public static WeirdOperatorOverride operator ^(WeirdOperatorOverride left, WeirdOperatorOverride right) => throw new NotImplementedException();
}
