using System;
using System.Collections.Generic;

class CompareNaN
{
    void ListPattern()
    {
        var baseCase = 2 == double.NaN; // Noncompliant

        var isPattern2 = 42D is not double.NaN; // Compliant, IsPattern is excluded 

        double[] numbers = new double[] { 1, double.NaN };
        var listPattern1 = numbers  is [not double.NaN, double.NaN]; // Compliant, IsPattern is excluded 
    }
}
