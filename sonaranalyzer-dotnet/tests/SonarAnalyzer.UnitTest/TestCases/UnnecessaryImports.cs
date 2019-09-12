using System.Collections.Concurrent; // Noncompliant {{Remove this unnecessary 'using'.}}
using System.IO;
using System.IO; // Noncompliant {{Remove this duplicate 'using'.}}
using static System.Console;
using static System.DateTime; // FN - System.DateTime is not a namespace symbol
using MySysAlias = System;
using MyOtherAlias = System.Collections; // FN - aliases not yet supported
using MyNamespace1; // Compliant - used inside MyNamspace2 to access Ns1_1
using MyNamespace3; // Noncompliant {{Remove this unnecessary 'using'.}}
using System.Collections.Generic;
using System.Globalization;

namespace MyNamespace0
{
    class Ns0_0 { }
}

namespace MyNamespace1
{
    class Ns1_0 { }
}

namespace MyNamespace2
{
    class Ns2_0
    {
        Ns1_0 ns11;
    }
}

namespace MyNamespace2.Level1
{
    using MyNamespace0;
    using MyNamespace0; // Noncompliant {{Remove this duplicate 'using'.}}
    using MyNamespace0; // Noncompliant {{Remove this duplicate 'using'.}}
    using MyNamespace1; // Noncompliant {{Remove this duplicate 'using'.}}
    using System.Linq; // Noncompliant {{Remove this unnecessary 'using'.}}
    using MyNamespace2.Level1; // Noncompliant {{Remove this unnecessary 'using'.}}
    using MyNamespace2; // Noncompliant {{Remove this unnecessary 'using'.}}

    class Ns2_1
    {
        Ns0_0 ns00;
        Ns2_0 ns20;
        Ns1_0 ns11;
        Ns2_1 ns21;
    }

    namespace Level2
    {
        using MyNamespace1; // Noncompliant {{Remove this duplicate 'using'.}}
        using System.IO; // Noncompliant {{Remove this duplicate 'using'.}}

        class Ns2_2
        {
            Ns1_0 ns11;

            void M(IEnumerable<DateTimeStyles> myEnumerable)
            {
                File.ReadAllLines("");
                WriteLine("");
                MySysAlias.Console.WriteLine("");
            }
        }
    }
}

namespace MyNamespace2.Level1.Level2
{
    using MyNamespace0;
    using MyNamespace2.Level1; // Noncompliant {{Remove this unnecessary 'using'.}}

    class Ns2_3
    {
        Ns0_0 ns00;
        Ns2_1 ns21;
    }
}

namespace MyNamespace3
{
    class Ns3_0 { }
}
