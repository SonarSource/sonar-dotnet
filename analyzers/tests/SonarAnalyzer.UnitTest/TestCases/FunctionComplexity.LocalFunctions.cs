using System;

namespace Tests.Diagnostics
{
    public class FunctionComplexity
    {
        public void MethodWithLocalfunctions() // Noncompliant [0]
                                              // Secondary@-1 [0] {{+1}}
        {
            void LocalFunction()
            {
                if (false) { }  // Secondary [0] {{+1}}
                if (false) { }  // Secondary [0] {{+1}}
                if (false) { }  // Secondary [0] {{+1}}
            }

            static void StaticLocalFunctions()
            {
                if (false) { }
                if (false) { }
                if (false) { }
            }
        }
    }
}
