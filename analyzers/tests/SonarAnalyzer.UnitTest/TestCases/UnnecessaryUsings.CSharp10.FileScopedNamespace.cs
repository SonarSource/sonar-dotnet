using System;

namespace MyNamespace0;

using MySysAlias = System;
using System.Linq; // Noncompliant {{Remove this unnecessary 'using'.}}
using System.Collections; // Noncompliant
using System.Globalization; // Noncompliant
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
