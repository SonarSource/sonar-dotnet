using System;

namespace MyNamespace0;

using MySysAlias = System;
using System.Linq; // FN
using System.Collections; // FN
using System.Globalization; // FN
using System.Collections.Generic;
using static System.Console;

class C
{
    MySysAlias.StringComparer x;

    void M(IEnumerable<string> myEnumerable)
    {
        WriteLine("");
        MySysAlias.Console.WriteLine("");
    }
}
