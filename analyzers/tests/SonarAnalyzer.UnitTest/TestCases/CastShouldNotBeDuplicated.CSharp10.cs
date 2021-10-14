using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    class Fruit { public List<int> Prop; }
    class Program
    {
        public void Foo(Object x, Object y)
        {
            if (x is Fruit { Prop.Count: 1 } tuttyFrutty)    // Secondary [property-pattern-1]
                                                             // Noncompliant@-1 [property-pattern-2] {{Remove this cast and use the appropriate variable.}}
            {
                var aRealFruit = (Fruit)tuttyFrutty;         // Noncompliant [property-pattern-1] {{Remove this redundant cast.}}
                var anotherFruit = (Fruit)x;                 // Secondary [property-pattern-2]
            }
        }
    }
}
