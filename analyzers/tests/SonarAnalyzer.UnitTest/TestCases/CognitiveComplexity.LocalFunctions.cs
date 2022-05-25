using System;

namespace TestCases
{
    class TestClass
    {
        void Foo(int x) // Noncompliant {{Refactor this method to reduce its Cognitive Complexity from 4 to the 0 allowed.}}
        {
            if (x == 0) // Secondary  {{+1}}
            {
                return;
            }

            void LocalFunction(int x)
            {
                if (x == 0) // Secondary
                {
                    Console.WriteLine(x);
                }
                else // Secondary
                {
                    return ;
                }
            }

            static void StaticLocalFunction(int x) // Static local functions are excluded from complexity computation.
                                                   // See issue https://github.com/SonarSource/sonar-dotnet/issues/5625
            {
                if (x == 0)
                {
                    Console.WriteLine(x);
                }
                else if (x > 10 && x < 100)
                {
                    Console.WriteLine(x);
                }
                else
                {
                    return;
                }
            }
        }
    }
}
