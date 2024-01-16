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
