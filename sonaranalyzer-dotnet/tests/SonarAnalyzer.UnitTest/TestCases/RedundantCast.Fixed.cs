using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests.Diagnostics
{
    public class RedundantCast
    {
        void foo(long l)
        {
            var x = new int[] {1, 2, 3}; // Fixed
            x = new int[] {1, 2, 3};
            x = x
; //Fixed
            x = x; //Fixed

            var y = x.OfType<object>();

            var zz = (int) l;
            int i = 0;
            var z = (int) i; // Fixed
            z = (Int32) i; // Fixed

            var w = (object) i;

            method(new int[] { 1, 2, 3 }); // Fixed
        }
        void method(IEnumerable<int> enumerable)
        { }

        void M()
        {
            var o = new object();
            var oo = o; // Fixed
            var i = o as RedundantCast; // Compliant
        }
    }
}
