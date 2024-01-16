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
            var x = d >>> d; // Compliant
            x = d >>> new MyClass(); // Compliant
            x = d >>> new MyUnknownClass(); // Compliant // Error [CS0246] - unknown type
        }
    }
}
