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

        if (numbers is [1, double.NaN, 3]) // FN
        {
            Console.WriteLine("Test2");
        }
    }
}

public interface ISomeInterface
{
    static virtual void StaticVirtualMembersInInterfaces()
    {
        var a = double.NaN;

        if (a == double.NaN) // Noncompliant {{Use double.IsNaN() instead.}}
        {
            Console.WriteLine("a is not a number");
        }
    }
}
