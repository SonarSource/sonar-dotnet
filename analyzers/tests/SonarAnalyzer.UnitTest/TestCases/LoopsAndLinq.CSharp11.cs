using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    class IsPatternTests
    {
        void ListPattern(List<int[]> list)
        {
            foreach (int[] array in list) // Noncompliant
            {
                if (array is [1, 2, 3]) // Secondary
                {
                    Console.WriteLine("Pattern match successful");
                }
            }

            foreach (var array in list) // Compliant, do not raise on VarPattern in ListPattern
            {
                if (array is [1, var x, var z])
                {
                    Console.WriteLine("Pattern match successful");
                }

            }

            foreach (var array in list) // Compliant, do not raise on declaration statements in ListPattern 
            {
                if (array is [1, ..] local)
                {
                    Console.WriteLine("Pattern match successful");
                }

            }
        }
    }
}
