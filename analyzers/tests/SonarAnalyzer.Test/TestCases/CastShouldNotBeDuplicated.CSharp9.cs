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
            object o;
            switch (x)                            // Noncompliant [switch-st-0] {{Remove this cast and use the appropriate variable.}}
            //      ^
            {
                case Water u:
                    o = (Water)x;                 // Secondary [switch-st-0]
                    break;
                default:
                    o = null;
                    break;
            }

            var message = x switch                 // Noncompliant [switch-expression-1] {{Remove this cast and use the appropriate variable.}}
            {
                Water w12 =>
                    ((Water)x).ToString(),         // Secondary [switch-expression-1]
                _ => "More than 10"
            };
        }

        public void MultipleCastsDifferentBlocks(object arg)
        {
            _ = (Fruit)arg;

            _ = 42 switch
            {
                1 => (Fruit)arg, // Compliant, not the same block
                2 => (Fruit)arg, // Compliant, not the same block
                _ => null
            };
        }
    }
}
