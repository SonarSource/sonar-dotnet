using System;
using System.Collections.Generic;

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
