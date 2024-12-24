using System;

namespace Tests.Diagnostics
{
    public class Program
    {
        public Program()
        {
            for (int l = 0; l < 10; l++)
            {
            }

            for (char c = 'a'; c <= 'z'; c++)
            {
            }

            int i;
            int j = 10;
            for (i = 0, Console.WriteLine("Start: {0}", i); i < j; i++, j--, Console.WriteLine("i={0}, j={1}", i, j))
            {
                // Body of the loop.
            }

            for (int k = 0; k < 10;) // Compliant - only the incrementor is missing
            {
                k++;
            }

            int z = 42;
            for (; z < 50; z++) // Compliant - only the declaration is missing
            {
            }

            var x = 5;
            for (; x < 10;) // Noncompliant {{Replace this 'for' loop with a 'while' loop.}}
//          ^^^
            {
            }

            for (; ; ) // Noncompliant
            {

            }

            for (i = 0; i < 10;) // Noncompliant
            {

            }
        }
    }
}
