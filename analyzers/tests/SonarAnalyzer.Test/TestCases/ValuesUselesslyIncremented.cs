using System;
using System.Collections;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public class ValuesUselesslyIncremented
    {
        public int pickNumber()
        {
            int i = 0;
            int j = 0;

            i = i++;    // Noncompliant; i is still zero
            i += i++;   // Compliant

            return j++; // Noncompliant {{Remove this increment or correct the code not to waste it.}}
//                 ^^^
        }

        public int pickNumber2()
        {
            int i = 0;
            int j = 0;

            i++;
            return ++j;
        }

        int x = 0;
        int y = 0;
        public int pickNumber3()
        {
            y = y++;
            return x++; // Compliant; 0 returned
        }
        public int pickNumber3(int i)
        {
            return i++; // Noncompliant
        }
        public int pickNumber4(ref int i)
        {
            return i++; // Compliant
        }

        public int pickNumber4(int i) => i++; // Noncompliant
    }

    class Repro_2265
    {
        public int i { get; set; }
        int Method(int i)
        {
            new Repro_2265 { i = i++ }; // Noncompliant FP
            return i;
        }
    }
}
