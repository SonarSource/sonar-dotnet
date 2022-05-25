using System;

namespace Tests.Diagnostics
{
    public class Foo
    {
        public void Bar(int i) // Noncompliant {{This method 'Bat' has 12 lines, which is greater than the 4 lines authorized. Split it into smaller methods.}}
        {
            Console.WriteLine(i);

            void LocalFunction(int x)
            {
                Console.WriteLine(x);
            }

            static void StaticLocalFunction()
            {
                int i = 0;
                i++;
                i++;
                i++;
            }
        }
    }
}
