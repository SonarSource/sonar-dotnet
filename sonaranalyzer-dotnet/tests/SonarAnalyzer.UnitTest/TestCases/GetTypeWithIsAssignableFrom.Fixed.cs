using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    class GetTypeWithIsAssignableFrom
    {
        void Test(bool b)
        {
            var expr1 = new GetTypeWithIsAssignableFrom();
            var expr2 = new GetTypeWithIsAssignableFrom();

            if (expr1.GetType()/*abcd*/.IsInstanceOfType(expr2 /*efgh*/)) //Fixed
            { }
            if (expr1.GetType().IsInstanceOfType(expr2)) //Compliant
            { }

            if (!(expr1 != null)) //Fixed
            { }
            var x = expr1 != null; //Fixed
            if (expr1 != null) // Fixed
            { }

            if (typeof(GetTypeWithIsAssignableFrom).IsAssignableFrom(typeof(GetTypeWithIsAssignableFrom))) //Compliant
            { }

            var t1 = expr1.GetType();
            var t2 = expr2.GetType();
            if (t1.IsAssignableFrom(t2)) //Compliant
            { }
            if (t1.IsInstanceOfType(expr2)) //Fixed
            { }

            if (t1.IsAssignableFrom(typeof(GetTypeWithIsAssignableFrom))) //Compliant
            { }

            Test(t1.IsInstanceOfType(expr2)); //Fixed
        }
    }
    class Fruit { }
    sealed class Apple : Fruit { }

    class Program
    {
        static void Main()
        {
            var apple = new Apple();
            var b = apple != null; // Fixed
            b = apple != null; // Fixed
            b = apple != null; // Fixed
            b = apple != null; // Fixed
            var appleType = typeof(Apple);
            b = appleType.IsInstanceOfType(apple); // Fixed

            b = apple.GetType() == typeof(int?); // Compliant

            Fruit f = apple;
            b = true && (f is Apple); // Fixed
            b = !(f is Apple); // Fixed
            b = f as Apple == new Apple();

            b = true && (apple != null); // Fixed
            b = !(apple != null); // Fixed
            b = f is Apple;

            var num = 5;
            b = num is int?;
            b = num is float;
        }
    }
}
