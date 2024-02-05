using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    abstract class Fruit { }
    class Apple : Fruit { }
    class Orange : Fruit { }

    class Program
    {
        // Error@+1 [CS0029]
        public static object[] os = new int[0]; // Noncompliant {{Refactor the code to not rely on potentially unsafe array conversions.}}
//                                  ^^^^^^^^^^
        public static object[] os2 = new object[0];

        static void Main(string[] args)
        {
            Fruit[] fruits = new Apple[1]; // Noncompliant - array covariance is used
//                           ^^^^^^^^^^^^
            fruits = new Apple[1]; // Noncompliant
            FillWithOranges(fruits);
            var fruits2 = new Apple[1];
            FillWithOranges(fruits2); // Noncompliant
            var fruits3 = (Fruit[])new Apple[1]; // Noncompliant
        }

        static void FillWithOranges(Fruit[] fruits)
        {
        }
    }
}
