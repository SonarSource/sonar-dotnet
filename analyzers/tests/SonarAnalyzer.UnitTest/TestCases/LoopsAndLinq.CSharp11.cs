using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests.Diagnostics
{
    class MyClass
    {
        void IsPattern(List<int> list)
        {
            foreach (var item in list) // Noncompliant
            {
                if (item is 42) // Secondary
                {
                    Console.WriteLine("The meaning of Life.");
                }
            }
        }

        void ListPattern(List<int[]> list)
        {
            foreach (int[] array in list) // Noncompliant
            {
                if (array is [1, 2, 3]) // Secondary
                {
                    Console.WriteLine("Pattern match successful");
                }
            }
        }
    }
}

