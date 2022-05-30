using System;

namespace Tests.Diagnostics
{
    public class Foo
    {
        public void Bar(int i) // Compliant, because we do not count the local static functions lines.
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
