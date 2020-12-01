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
            Fruit[] fruits1 = isTrue ? new Apple[1] : new Orange[1]; // FN

            Fruit[] fruits2 = apples ?? givenFruits; // FN

            Fruit[] fruits3 = givenFruits ?? apples; // FN
        }
    }
}
