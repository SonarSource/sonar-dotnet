using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    class Program
    {
        public void Foo()
        {
            if (x is Fruit)  // Noncompliant
            {
                var f = (Fruit)x;
//                      ^^^^^^^^ Secondary
            }

            var f = x as Fruit;
            if (x != null) // Compliant
            {

            }
        }

        public void Bar()
        {
            if (!(x is Fruit))
            {
                var f1 = (Fruit)x; // Compliant - but will throw
            }
            else
            {
                var f2 = (Fruit)x; // Compliant - should be non compliant
            }

        }
    }
}
