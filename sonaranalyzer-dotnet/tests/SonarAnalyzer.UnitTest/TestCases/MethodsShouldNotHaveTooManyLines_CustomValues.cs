using System;

namespace Tests.Diagnostics
{
    class Program
    {
        public Program() // Noncompliant {{This constructor 'Program' has 4 lines, which is greater than the 2 lines authorized. Split it into smaller methods.}}
//             ^^^^^^^
        {
            int i = 1; i++;
            i++;
            i++;
            i++;
        }

        public ~Program() // Noncompliant {{This finalizer '~Program' has 4 lines, which is greater than the 2 lines authorized. Split it into smaller methods.}}
        {
            int i = 1; i++;
            i++;
            i++;
            i++;
        }


        public void Method_01() // Noncompliant {{This method 'Method_01' has 4 lines, which is greater than the 2 lines authorized. Split it into smaller methods.}}
//                  ^^^^^^^^^
        {
            int i = 1; i++;
            i++;
            i++;
            i++;
        }

        public int Method_02() // Noncompliant {{This method 'Method_02' has 13 lines, which is greater than the 2 lines authorized. Split it into smaller methods.}}
        {
            int i = 1;
            i++;
            i++;
            i++;
            i++;
            i++;
            i++;
            i++;
            i++;
            i++;
            i++;
            i++;

            return 1;
        }

        public abstract int Method_04();

        extern int Method_05();

        public int Method_06() // Noncompliant {{This method 'Method_06' has 6 lines, which is greater than the 2 lines authorized. Split it into smaller methods.}}
        {

            void InnerFunction_06()
            {
                int i = 0;
                i++;
                i++;
            }
        }

        public string Method_07() // Noncompliant {{This method 'Method_07' has 4 lines, which is greater than the 2 lines authorized. Split it into smaller methods.}}
            =>
            1
            .ToString()
            .ToString()
            .ToString();

        public string Method_08()
            =>
            1
            .ToString();

        // Compliant. Properties are not covered by this rule
        public int Property_01
        {
            get
            {
                int i = 0;
                i++;

                return 1;
            }

            set
            {
                int i = 0;
                i++;
                i++;
            }
        }

    }
}
