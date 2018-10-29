using System;

namespace Tests.Diagnostics
{
    abstract class Program
    {
        public Program() // Noncompliant {{This constructor 'Program' has 4 lines, which is greater than the 2 lines authorized. Split it into smaller methods.}}
//             ^^^^^^^
        {
            int i = 1; i++;
            i++;
            i++;
            i++;
        }

        ~Program() // Noncompliant {{This finalizer '~Program' has 4 lines, which is greater than the 2 lines authorized. Split it into smaller methods.}}
        {
            int i = 1; i++;
            i++;
            i++;
            i++;
        }


        public void Method_01() // Noncompliant {{This method 'Method_01' has 4 lines, which is greater than the 2 lines authorized. Split it into smaller methods.}}
//                  ^^^^^^^^^
        {
            for (int i = 1; i < 10; i++)
            {
                i++;
            }
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

        public int Method_06() // Noncompliant {{This method 'Method_06' has 7 lines, which is greater than the 2 lines authorized. Split it into smaller methods.}}
        {
            // We only report on outer methods.
            // The lines of code of inner functions are counted against the method.
            void InnerFunction_06()
            {
                int i = 0;
                i++;
                i++;
            }

            return 1;
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

        public string Method_09()
        {
            /*
             * comments are not counted, y'know?
             *
             */

            //
            // Neither are these
            //

            return null;
        }

        public string Method_10() // Noncompliant {{This method 'Method_10' has 3 lines, which is greater than the 2 lines authorized. Split it into smaller methods.}}
        { int i = 0;
            i++;
            return null; }

        public string Method_11() // Noncompliant {{This method 'Method_11' has 5 lines, which is greater than the 2 lines authorized. Split it into smaller methods.}}
        {
            return @"



            ";
        }

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
