using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    class MyClass
    {
        void ListPattern(List<int[]> list)
        {
            foreach (int[] array in list) // FN
            {
                if (array is [1, 2, 3]) // FN
                {
                    Console.WriteLine("Test");
                }
            }
        }
    }
}
