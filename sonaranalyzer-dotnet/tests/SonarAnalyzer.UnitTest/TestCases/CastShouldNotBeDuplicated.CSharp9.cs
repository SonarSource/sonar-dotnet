using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    class Fruit { }
    class Vegetable { }

    class Program
    {
        private object someField;

        public void Foo(Object x, Object y)
        {
            var fruit = x as Fruit;
            if (fruit is not Fruit) // FN
            {
            }

            object o;
            switch (x)
            {
                case Fruit m:
                    o = (Fruit)m; // FN
                    break;
                case Vegetable t when t != null:
                    o = (Vegetable)t; // FN
                    break;
                default:
                    o = null;
                    break;
            }

            if ((x, y) is (Fruit f, Vegetable v))
            {
                var ff = (Fruit)f; // FN
                var vv = (Vegetable)v; // FN
            }

            if (x is Fruit f2)
            {
                var ff = (Fruit)f2; // FN
            }

            var message = x switch
            {
                Fruit f3 => ((Fruit)f3).ToString(), // FN
                Vegetable v3 => ((Vegetable)v3).ToString(), // FN
                _ => "More than 10"
            };
        }

        public void Bar(object x)
        {
            if (x is not Fruit)
            {
                var f1 = (Fruit)x; // Compliant - but will throw
            }
            else
            {
                var f2 = (Fruit)x; // Compliant
            }
        }

        public void FooBar(object x)
        {
            if (x is nuint)
            {
                var res = (nuint)x; // Compliant because we are casting to a ValueType
            }
        }

        // See https://github.com/SonarSource/sonar-dotnet/issues/2314
        public void TakeIdentifierIntoAccount(object x)
        {
            if (x is Fruit)
            {
                Fruit f = new();
                var c = (Fruit)f;
            }
        }

    }
}
