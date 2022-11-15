using System;
using System.Collections.Generic;

class MyClass
{
    void ListPattern()
    {
        object[] numbers = { 1, 2, 3 };

        if (numbers is [Fruit fruit, 3, 3])    // Secondary
//                      ^^^^^^^^^^^
        {
            var ff1 = (Fruit)fruit;            // Noncompliant
        }

        if (numbers is [double number, 3, 3])
        {
            var ff2 = (double)number;          // Compliant (casting to a ValueType)
        }

        if (numbers is [1, 2, 3] anotherNumber)
        {
            var ff3 = (object[])anotherNumber; // FN it will probably require a rule redesign
        }
    }
}

class Fruit { }
