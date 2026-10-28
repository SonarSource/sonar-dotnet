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
    void Compare(Decimal32 d32, Decimal64 d64, Decimal128 d128, BFloat16 bf16)
    {
        var a = d32 == Decimal32.NaN;      // FN NET-4880
        var b = d64 != Decimal64.NaN;      // FN NET-4880
        var c = d128 < Decimal128.NaN;     // FN NET-4880
        var d = bf16 == BFloat16.NaN;      // FN NET-4880
        var e = Decimal64.IsNaN(d64);
    }
}
