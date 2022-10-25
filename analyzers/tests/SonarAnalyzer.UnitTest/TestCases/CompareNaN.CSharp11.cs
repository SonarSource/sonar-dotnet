using System;
using System.Collections.Generic;

class CompareNaN
{
    void ListPattern()
    {
        object[] numbers = { 1, 2, double.NaN };

        if (numbers is [1, 2, double.NaN]) // FN
        {
            Console.WriteLine("Test1");
        }
    }
}
