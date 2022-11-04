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
            dynamic d = 5;
            var x = d >> 5.4; // Noncompliant
//                       ^^^
            x = d >> null; // Noncompliant
            x <<= new object(); // Noncompliant {{Remove this erroneous shift, it will fail because 'object' can't be implicitly converted to 'int'.}}

            x = d << d; // Compliant
            x = d >> new MyClass(); // Compliant

            x = d >> new MyUnknownClass(); // Compliant // Error [CS0246] - unknown type
        }
    }
}
