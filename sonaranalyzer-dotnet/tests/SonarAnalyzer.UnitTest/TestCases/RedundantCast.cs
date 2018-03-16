using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests.Diagnostics
{
    public class RedundantCast
    {
        void foo(long l)
        {
            var x = new int[] {1, 2, 3}.Cast<int>(); // Noncompliant
//                                     ^^^^^^^^^^^^
            x = Enumerable // Noncompliant
                .Cast<int>(new int[] {1, 2, 3});
            x = x
                .OfType<int>(); //Noncompliant
            x = x.Cast<int>(); //Noncompliant

            var y = x.OfType<object>();

            var zz = (int) l;
            int i = 0;
            var z = (int) i; // Noncompliant {{Remove this unnecessary cast to 'int'.}}
//                   ^^^
            z = (Int32) i; // Noncompliant {{Remove this unnecessary cast to 'int'.}}

            var w = (object) i;

            method(new int[] { 1, 2, 3 }.Cast<int>()); // Noncompliant {{Remove this unnecessary cast to 'IEnumerable<int>'.}}
        }
        void method(IEnumerable<int> enumerable)
        { }

        void M()
        {
            var o = new object();
            var oo = o as object; // Noncompliant
            var i = o as RedundantCast; // Compliant
        }

        public void N(int[,] numbers)
        {
            numbers.Cast<int>().Where(x => x > 0); // Compliant, multidimensional arrays need to be cast
        }
    }
}
