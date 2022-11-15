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

        var anotherFruit = new Fruit();
        switch (numbers) // Noncompliant, switch statement
        {
            case [(Fruit) anotherFruit, 2, 3]: // Secondary
                return;
            default:
                break;
        }

        var res = Fruit fruit switch // Noncompliant, switch expression
        {
            [Fruit fruit, 2, 3] => 1, // Secondary
            _ => 42
        };
    }
}

class Fruit { }
