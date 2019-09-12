using System.IO;
using static System.Console;
using static System.DateTime; // FN - System.DateTime is not a namespace symbol
using MySysAlias = System;
using MyOtherAlias = System.Collections; // FN - aliases not yet supported
using MyNamespace1; // Compliant - used inside MyNamspace2 to access Ns1_1
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

[assembly: AssemblyVersion("1.0.0.0")]

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

    class Ns2_1
    {
        Ns0_0 ns00;
        Ns2_0 ns20;
        Ns1_0 ns11;
        Ns2_1 ns21;
    }

    namespace Level2
    {

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
