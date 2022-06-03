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

            // Static local functions are excluded from the complexity computation of method
            // that they are nested in. They have their own complexity score as independent methods.
            // See issue https://github.com/SonarSource/sonar-dotnet/issues/5625
            static void StaticLocalFunction(int x) // Noncompliant  {{Refactor this static local function to reduce its Cognitive Complexity from 3 to the 0 allowed.}}
            {
                if (x == 0) // Secondary {{+2 (incl 1 for nesting)}}
                {
                    Console.WriteLine(x);
                }
                else // Secondary {{+1}}
                {
                    return;
                }
            }
        }
    }
}
