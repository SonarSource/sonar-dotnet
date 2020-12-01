using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    class Program
    {
        public void Method1(string method1, string METHOD1, string argument)
//                  ^^^^^^^ Secondary [0,1]
//                                 ^^^^^^^ Noncompliant@-1 [0] {{Rename the parameter 'method1' so that it does not duplicate the method name.}}
//                                                 ^^^^^^^ Noncompliant@-2 [1] {{Rename the parameter 'METHOD1' so that it does not duplicate the method name.}}
        {
            // Do something
        }

        public int @int(int @int) => 0; // Noncompliant
                                        // Secondary@-1

        public int @int(string @int1) => 0;
    }

    class WithLocalFunctions
    {
        public void Method()
        {
            void Method1(string Method1) // Noncompliant
            {                            // Secondary@-1
            }

            static void Method2(string Method2) // Noncompliant
            {                                   // Secondary@-1
            }

            void Method3(string Method4)
            {
            }
        }
    }
}
