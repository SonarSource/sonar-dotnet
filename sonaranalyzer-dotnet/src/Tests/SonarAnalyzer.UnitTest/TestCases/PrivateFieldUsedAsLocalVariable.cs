using System;

namespace Tests.Diagnostics
{
    class PrivateFieldUsedAsLocalVariable
    {
        private int F0 = 0; // Compliant - unused

        private int F1 = 0; // Noncompliant {{Remove the 'F1' field and declare it as a local variable in the relevant methods.}}
//                  ^^^^^^
        public int F2 = 0; // Compliant - Public

        private int F3 = 1; // Compliant - Referenced from another field initializer
        public int F4 = F3;

        private int F5 = 0; // Noncompliant
        private int F6; // Noncompliant

        private int F7 = 0; // Compliant - Read before write

        private int F8 = 0; // Noncompliant
        private int F9 = 0; // Noncompliant
        private int F10 = 0; // Compliant - not assigned from every path

        private int F11 = 0; // Compliant - first read through 'this.'
        private int F12 = 0; // Noncompliant

        private int F13 = 0; // Compliant - parameter of same name is assigned, not the field
        private int F14 = 0; // Noncompliant

        private int F15 = 0; // Compliant - returned in property getter
        private int F16 = 0; // Noncompliant

        private static int F17 = 0; // Compliant - auto-property
        private int F18 = 0; // Compliant - arrow method

        private int F19 = 42; // Compliant - accessed through instance
        private int F20 = 42; // Compliant - accessed through instance
        private int F21 = 42; // Compliant - accessed through instance
        private static int F22 = 42; // Noncompliant
        private int F23 = 42; // Noncompliant - access through this?.

        private int F24 = 42; // Noncompliant - passed as 'out'
        private int F25 = 42; // Compliant - passed as 'ref'

        private int F26 = 42; // Noncompliant - always assigned from constructor
        private int F27 = 42; // Compliant - passed to another constructor

        private int F28 = 42; // Noncompliant - always assigned from event
        private int F29 = 42; // Compliant

        private int F30 = 42; // Noncompliant - always assigned from 2 methods
        private int F31 = 42; // Compliant

        private string F32 = ""; // Noncompliant
        private static string F33 = ""; // Noncompliant
        private string F34 = ""; // Compliant

        [Obsolete]
        private string F35 = ""; // Compliant - has attribute

        PrivateFieldUsedAsLocalVariable() : this(F27)
        {
            F26 = 42;
        }

        PrivateFieldUsedAsLocalVariable(int a)
        {
        }

        void M1()
        {
            F1 = 42;
            F2 = 42;
        }

        void M2()
        {
            F5 = 42;
            F6 = 42;
        }

        void M3()
        {
            Console.WriteLine(F7);
            F7 = 42;
        }

        void M4(bool p1)
        {
            for (F8 = 0; F8 < 42; F8++) { }

            if (p1)
            {
                F9 = 0;
                F10 = 0;
            }
            else
            {
                F9 = 1;
            }
            Console.WriteLine(F9);
            Console.WriteLine(F10);
        }

        void M5()
        {
            Console.WriteLine(this.F11);
            F11 = 42;

            this.F12 = 42;
            Console.WriteLine(F12);
        }

        void M6(int F13, int F14)
        {
            F13 = 42;
            this.F14 = 42;
        }

        public int P1 { get { return F15; } }
        public int P2 { get { F16 = 42; return F16; } }

        public int P3 { get; set; } = F17;
        public int M7() => F18;

        void M8()
        {
            F17 = 42;
            F18 = 42;
        }

        void M9(PrivateFieldUsedAsLocalVariable inst)
        {
            F19 = inst.F19;

            if (inst.F20 == 42)
            {
                Console.WriteLine();
            }

            F20 = 0;

            inst.F21 = 42;
            if (F21 == 42)
            {
                Console.WriteLine();
            }

            PrivateFieldUsedAsLocalVariable.F22 = 42;
            Console.WriteLine(F22);

            this?.F23 = 42;
        }

        void M10()
        {
            M11(out F24, ref F25);
        }

        void M11(out int a, ref int b)
        {
            a = 42;
            b = 42;
        }

        event EventHandler E1
        {
            add
            {
                F28 = 42;
                F29 = 42;
            }
            remove
            {
                Console.WriteLine(F29);
            }
        }

        void M12()
        {
            F30 = 42;
            F31 = 40;
        }

        void M13()
        {
            this.F32 = 42;
            Console.WriteLine(this.F31);
        }

        ~PrivateFieldUsedAsLocalVariable()
        {
            this.F33 = "foo";
        }

        public static PrivateFieldUsedAsLocalVariable operator +(PrivateFieldUsedAsLocalVariable c1, PrivateFieldUsedAsLocalVariable c2)
        {
            PrivateFieldUsedAsLocalVariable.F33 = "foo";
            c1.F34 = "foo";
            return null;
        }

        void M14()
        {
            this.F35 = "foo";
        }
    }

    public partial class SomePartialClass
    {
        private int F0 = 0; // Compliant - partial classes are not checked

        void M1()
        {
            F0 = 0;
            Console.WriteLine(F0);
        }
    }

    public struct SomeStruct
    {
        private int F0 = 0; // Noncompliant

        void M1()
        {
            F0 = 0;
            Console.WriteLine(F0);
        }
    }
}
