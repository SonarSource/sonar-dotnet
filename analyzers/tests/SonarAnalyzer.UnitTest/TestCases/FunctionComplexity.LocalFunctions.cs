using System;

namespace Tests.Diagnostics
{
    public class FunctionComplexity
    {
        public void MethodWithLocalfunctions() // Noncompliant [0] {{The Cyclomatic Complexity of this method is 4 which is greater than 3 authorized.}}
                                               // Secondary@-1 [0] {{+1}}
        {
            void LocalFunction()
            {
                if (false) { }  // Secondary [0] {{+1}}
                if (false) { }  // Secondary [0] {{+1}}
                if (false) { }  // Secondary [0] {{+1}}
            }

            static void StaticLocalFunctions() // Noncompliant [1] {{The Cyclomatic Complexity of this static local function is 5 which is greater than 3 authorized.}}
                                               // Secondary@-1 [1] {{+1}}
            {
                if (false) { } // Secondary [1] {{+1}}
                if (false) { } // Secondary [1] {{+1}}
                if (false) { } // Secondary [1] {{+1}}
                if (false) { } // Secondary [1] {{+1}}
            }
        }
    }
}
