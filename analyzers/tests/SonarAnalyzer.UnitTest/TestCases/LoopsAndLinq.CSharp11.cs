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

        void OtherIsPatterns(List<string> strings, List<Tuple<string, int>> tuples)
        {
            const string target = "42";

            foreach (var s in strings) // Noncompliant
            {
                if (s is target) // Secondary 
                {
                    Console.WriteLine("Pattern match successful");
                }
            }

            foreach (var s in strings) // Compliant, do not raise on VarPattern in IsPattern
            {
                if (s is var s2)
                {
                    Console.WriteLine("Pattern match successful");
                }
            }

            foreach (var s in strings) // Compliant, do not raise on SingleVariableDeclaration in IsPattern
            {
                if (s is { Length: 42 } str) 
                {
                    Console.WriteLine("Pattern match successful");
                }
            }

            foreach (var t in tuples) // Compliant, do not raise on ParenthesizedVariableDeclaration in IsPattern
            {
                if (t is var (t1, t2)) 
                {
                    Console.WriteLine("Pattern match successful");
                }
            }
        }
    }
}
