using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    class Fruit { }
    class Vegetable { }
    struct Water { }
    class Foo { public int x; }

    class Program
    {
        private object someField;

        public void Foo(Object x, Object y)
        {
            var fruit = x as Fruit;
            if (fruit is not Fruit) // Compliant, redundant condition, not related for the current rule
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

            if ((x, y) is (Fruit f1, Vegetable v1))   // Secondary
            {
                var ff1 = (Fruit)f1;                  // Noncompliant
            }

            if ((x, y) is (Fruit f2, Vegetable v2))   // Secondary
            {
                var ff2 = (Vegetable)v2;              // Noncompliant
            }

            if ((x, y) is (Fruit f3, Vegetable v3))   // Noncompliant
            {
                var ff3 = (Fruit)x;                   // Secondary
            }

            if ((x, y) is (Fruit f4, Vegetable v4))   // Noncompliant
            {
                var ff4 = (Vegetable)y;               // Secondary
            }

            if ((x,y) is (Fruit f5, Vegetable v5, Vegetable v51)) // Error [CS8502]
            {
                var ff5 = (Fruit)x;
            }

            if (x is Fruit f6)          // Secondary
            {
                var ff6 = (Fruit)f6;    // Noncompliant {{Remove this redundant cast.}}
            }

            if (x is Fruit f7)          // Noncompliant {{Replace this type-check-and-cast sequence with an 'as' and a null check.}}
            {
                var ff7 = (Fruit)x;     // Secondary
            }

            if (x is UnknownFruit f8)   // Error [CS0246]
            {
                var ff8 = (Fruit)x;
            }

            if (x is Water f9)
            {
                var ff9 = (Fruit)x;
            }

            x is Fruit f0; // Error [CS0201]

            var message = x switch
            {
                Fruit f10 => ((Fruit)f10).ToString(), // FN
                Vegetable v11 => ((Vegetable)v11).ToString(), // FN
                _ => "More than 10"
            };

            if ((x) is (Fruit f12, Vegetable v12))
            {
                var ff12 = (Vegetable)x;               // FN
            }

            Foo k = null;
            if (k is { x : 0 })
            {
            }
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
