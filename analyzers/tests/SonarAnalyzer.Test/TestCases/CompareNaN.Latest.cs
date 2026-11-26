using System;
using System.Collections.Generic;
using System.Numerics;

class CompareNaN
{
    void ListPattern()
    {
        var baseCase = 2 == double.NaN; // Noncompliant

        double[] numbers = new double[] { 1, double.NaN };
        var listPattern1 = numbers is [not double.NaN, double.NaN]; // Compliant, IsPattern is excluded, works as expected
        var listPattern2 = numbers is not [not double.NaN, double.NaN]; // Compliant, IsPattern is excluded, works as expected
    }
}


class NullConditionalAssignment
{
    class MyClass
    {
        public bool Valid { get; set; }
    }
    public void TestMethod()
    {
        var obj = new MyClass();
        double number = 42;
        obj?.Valid = double.NaN >= number;  // Noncompliant
    }
}

class IeeeDecimalAndBFloat16
{
    void Compare(Decimal32 d32, BFloat16 bf16)
    {
        var a = d32 == Decimal32.NaN;      // Noncompliant {{Use Decimal32.IsNaN() instead.}}
        var b = bf16 >= BFloat16.NaN;      // Noncompliant {{Do not compare a number with BFloat16.NaN.}}
        var c = Decimal32.IsNaN(d32);
    }
}

class GenericAndCustom
{
    void Generic<T>(T t) where T : IFloatingPointIeee754<T>
    {
        var a = t == T.NaN;                // Noncompliant {{Use T.IsNaN() instead.}}
        var b = T.IsNaN(t);
    }

    void UserDefined<T>(T t) where T : ICustomIeee<T>
    {
        var a = t != T.NaN;                // Noncompliant {{Use T.IsNaN() instead.}}
    }
}

interface ICustomIeee<T> : IFloatingPointIeee754<T> where T : ICustomIeee<T>
{
}

class HalfAndNFloat
{
    void Compare(Half half, System.Runtime.InteropServices.NFloat nfloat)
    {
        var a = half == Half.NaN;               // Noncompliant - NET-4880
        var b = nfloat != System.Runtime.InteropServices.NFloat.NaN; // Noncompliant - NET-4880
        var c = Half.IsNaN(half);
    }
}
