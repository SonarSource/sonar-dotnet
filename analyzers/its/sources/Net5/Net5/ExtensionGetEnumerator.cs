using System;
using System.Collections.Generic;

namespace Net5
{
    public static class MyExtensions
    {
        public static IEnumerable<int> GetEnumerator(this Range range)
        {
            for (var i = range.Start.Value; i < range.End.Value; i++)
            {
                yield return i;
            }
        }
    }

    public class ExtensionGetEnumerator
    {

        public void Method()
        {
            var range = 1..2;
            foreach (var i in range.GetEnumerator())
            {
                Console.WriteLine($"Print with explicit GetEnumerator {i}");
            }

            // The feature is in progress

            //foreach (var i in range)
            //{
            //    Console.WriteLine($"Print with implicit GetEnumerator {i}");
            //}
        }
    }
}
