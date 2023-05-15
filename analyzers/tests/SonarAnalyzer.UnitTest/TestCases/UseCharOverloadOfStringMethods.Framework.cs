using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

class Testcases
{
    void Simple()
    {
        var str = "hello";

        str.StartsWith("x"); // Compliant
        str.EndsWith("x"); // Compliant

        str.StartsWith('x'); // Error
        str.EndsWith('x'); // Error
    }
}
