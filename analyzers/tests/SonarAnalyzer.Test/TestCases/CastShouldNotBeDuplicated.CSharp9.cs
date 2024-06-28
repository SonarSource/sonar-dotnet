using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    class Fruit { public int Prop; }
    class Vegetable { }
    struct Water { }
    class Foo { public int x; }
    class Complex { public object x; }

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
            switch (x)                            // Noncompliant [switch-st-0] {{Remove this cast and use the appropriate variable.}}
            //      ^
            {
                case Fruit m:                     // Secondary [switch-st-1]
            //       ^^^^^^^
                    o = (Fruit)m;                 // Noncompliant [switch-st-1] {{Remove this redundant cast.}}
                    break;
                case Vegetable t when t != null:  // Secondary [switch-st-2]
                    o = (Vegetable)t;             // Noncompliant [switch-st-2] {{Remove this redundant cast.}}
                    break;
                case Water u:
                    o = (Water)x;                 // Secondary [switch-st-0]
                    break;
                default:
                    o = null;
                    break;
            }

            if ((x, y) is (Fruit f1, Vegetable v1))   // Secondary
            //             ^^^^^^^^
            {
                var ff1 = (Fruit)f1;                  // Noncompliant
                //        ^^^^^^^^^
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
                var fff6 = (Vegetable)x;
            }

            if (x is Fruit f7)          // Noncompliant {{Remove this cast and use the appropriate variable.}}
            {
                var ff7 = (Fruit)x;     // Secondary
                var fff7 = (Vegetable)x;
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

            if (x is not Water)
            {
                var xWater = (Water)x;
            }
            else if (x is not Fruit)
            {
                var xFruit = (Fruit)x;
            }

            var message = x switch                 // Noncompliant [switch-expression-1] {{Remove this cast and use the appropriate variable.}}
            {
                Fruit f10 =>                       // Secondary [switch-expression-2]
            //  ^^^^^^^^^
                    ((Fruit)f10).ToString(),       // Noncompliant [switch-expression-2] {{Remove this redundant cast.}}
            //       ^^^^^^^^^^
                Vegetable v11 =>                   // Secondary [switch-expression-3]
                    ((Vegetable)v11).ToString(),   // Noncompliant [switch-expression-3]
                (string left, string right) =>     // Secondary [switch-expression-4, switch-expression-5]
                    (string) left + (string) right,// Noncompliant [switch-expression-4]
                                                   // Noncompliant@-1 [switch-expression-5]
                Water w12 =>
                    ((Water)x).ToString(),         // Secondary [switch-expression-1]
                Complex { x : Fruit apple } => "apple",
                _ => "More than 10"
            };

            if ((x) is (Fruit f12, Vegetable v12))     // Noncompliant
            {
                var ff12 = (Vegetable)x;               // Secondary
            }

            Foo k = null;
            if (k is { x : 0 })
            {
            }

            if (x is (Water f13))                      // Noncompliant
            {
                var ff13 = (Water)x;                   // Secondary
            }
        }

        public void Bar(object x, object y)
        {
            if (x is not Fruit)
            {
                var f1 = (Fruit)x; // Compliant - but will throw
            }
            else
            {
                var f2 = (Fruit)x; // Compliant
            }

            if (x is Fruit { Prop: 1 } tuttyFrutty)    // Secondary [property-pattern-1]
                                                       // Noncompliant@-1 [property-pattern-2] {{Remove this cast and use the appropriate variable.}}
            {
                var aRealFruit = (Fruit)tuttyFrutty;   // Noncompliant [property-pattern-1] {{Remove this redundant cast.}}
                var anotherFruit = (Fruit)x;           // Secondary [property-pattern-2]
            }

            var foo = new Complex();
            if (foo is {x: Fruit f })                  // Secondary
            //             ^^^^^^^
            {
                var something = (Fruit)f;              // Noncompliant
                //              ^^^^^^^^
            }

            if ((x, y) is (1))                                 // Error[CS0029]
            {
            }

            if ((x, y) is (R { SomeProperty: { Count: 5 } }))  // Error [CS0246]
            {
            }
        }

        public void FooBar(object x)
        {
            if (x is nuint)         // Noncompliant
            {
                var res = (nuint)x; // Secondary
            }
        }

        public void Baz(object x, object y)
        {
            if ((x, y) is ((Fruit a, Fruit b), string v)) // Secondary
                                                          // Secondary@-1
                                                          // Secondary@-2
            {
                var a1 = (Fruit)a;                        // Noncompliant
                var b1 = (Fruit)b;                        // Noncompliant
                var v1 = (string)v;                       // Noncompliant
            }

            if (x is (Fruit or Vegetable))            // Noncompliant
            {
                var fruit = (Fruit)x;                 // Secondary
            }

            if (x is (Fruit or Vegetable))            // Noncompliant
            {
                var vegetable = (Vegetable)x;         // Secondary
            }
        }

        public void NonExistingType()
        {
            if (x is Fruit f)                       // Error [CS0103]
                                                    // Secondary@-1
            {
                var ff = (Fruit)f;                  // Noncompliant {{Remove this redundant cast.}}
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
