using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{

    class UnaryPrefixOperatorRepeated
    {
        static void NonComp(bool bbb)
        {
            int i = 1;

            int k = ~~i; // Noncompliant; same as i
//                  ^^
            int m = + +i; // Compliant
            int n = - -i; // Compliant, we care only about !

            bool b = false;
            bool c = !!!b; // Noncompliant

            NonComp(!!!b); // Noncompliant {{Use the '!' operator just once or not at all.}}
        }

        static void Comp()
        {
            int i = 1;

            int j = -i;
            j = -(-i); // Compliant, not a typo
            int k = i;
            int m = i;

            bool b = false;
            bool c = !b;
        }
    }
}
