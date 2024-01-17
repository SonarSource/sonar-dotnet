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

            static void StaticLocalFunctionWithManyLines() // Noncompliant {{This static local function has 8 lines, which is greater than the 5 lines authorized.}}
            {
                int i = 0;
                i++;
                i++;
                i++;
                i++;
                i++;
                i++;
                i++;
            }
        }

        public void FooBar(int i) // Noncompliant {{This method 'FooBar' has 12 lines, which is greater than the 5 lines authorized. Split it into smaller methods.}}
        {
            Console.WriteLine(i);

            void LocalFunctionWithManyLines()
            {
                int i = 0;
                i++;
                i++;
                i++;
                i++;
                i++;
                i++;
                i++;
            }
        }
    }
}
