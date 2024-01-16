using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    abstract class Fruit { }
    class Apple : Fruit { }
    class Orange : Fruit { }

    class Program
    {
        static void TargetTypedConditional(bool isTrue, Apple[] apples, Fruit[] givenFruits)
        {
            Fruit[] fruits1 = isTrue
                ? new Apple[1] // Noncompliant {{Refactor the code to not rely on potentially unsafe array conversions.}}
//                ^^^^^^^^^^^^
                : new Orange[1]; // Noncompliant {{Refactor the code to not rely on potentially unsafe array conversions.}}
//                ^^^^^^^^^^^^^

            Fruit[] fruits2 = apples ?? givenFruits; // Noncompliant
//                            ^^^^^^

            Fruit[] fruits3 = givenFruits ?? apples; // Noncompliant
//                                           ^^^^^^

            AddToBasket(apples ?? givenFruits); // Noncompliant
//                      ^^^^^^

            AddToBasket(isTrue ? new Apple[1] : new Orange[1]); // Noncompliant
                                                                // Noncompliant@-1

            fruits1 = isTrue ? new Apple[1] : new Orange[1]; // Noncompliant
                                                             // Noncompliant@-1
            fruits1 = apples ?? givenFruits; // Noncompliant

            fruits1 = (Fruit[])(isTrue ? new Apple[1] : new Orange[1]); // Noncompliant
                                                                        // Noncompliant@-1
            fruits1 = (Fruit[])(apples ?? givenFruits); // Noncompliant
        }

        static void AddToBasket(Fruit[] fruits)
        {
        }
    }
}
