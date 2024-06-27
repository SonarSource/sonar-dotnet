using System;
using System.Collections.Generic;

class MyClass
{
    void ListPattern()
    {
        object[] numbers = { 1, 2, 3 };

        if (numbers is [Fruit fruit, 3, 3])     // Secondary
//                      ^^^^^^^^^^^
        {
            var ff1 = (Fruit)fruit;             // Noncompliant
        }

        if (numbers is [double number, 3, 3])   // Secondary
        {
            var ff2 = (double)number;           // Noncompliant
        }

        if (numbers is [1, 2, 3] anotherNumber)
        {
            var ff3 = (object[])anotherNumber;  // FN it will probably require a rule redesign
        }
    }
}

class Fruit { }

class SomeClass
{
    private object obj;

    public void SwitchStatement(object[] array)
    {
        switch (array)
        {
            case [Fruit m, 2]: // Secondary
//                ^^^^^^^
                obj = (Fruit)m; // Noncompliant
//                    ^^^^^^^^
                break;
            default:
                obj = null;
                break;
        }
    }

    public void SwitchExpression(object[] array) =>
        obj = array switch
        {
            [Fruit m, 2, 2] => // Secondary
//           ^^^^^^^
                (Fruit)m, // Noncompliant
//              ^^^^^^^^
            _ => null
        };
}
