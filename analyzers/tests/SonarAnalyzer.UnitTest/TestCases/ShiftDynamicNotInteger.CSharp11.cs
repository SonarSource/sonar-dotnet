using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public class MyClass
    {
        public static implicit operator int(MyClass self)
        {
            return 1;
        }
    }

    class ShiftDynamicNotInteger
    {
        public void Test()
        {
            int d = 5;
            var x = d >>> null; // FN
            int? y = 5;
            y >>>= null; // FN

            x = d >>> d; // okay
            x = d >>> new MyClass(); // okay

            x = d >>> new MyUnknownClass(); // Compliant // Error [CS0246] - unknown type
        }
    }
}
