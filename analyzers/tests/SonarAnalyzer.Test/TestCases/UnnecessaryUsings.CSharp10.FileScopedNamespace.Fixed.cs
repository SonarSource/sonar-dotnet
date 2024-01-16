using System;

namespace MyNamespace0;

using MySysAlias = System;
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
