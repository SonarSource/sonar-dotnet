using System;

namespace Tests.TestCases
{
    class MemberShadowsOuterStaticMember
    {
        private delegate void SomeName();

        const int a = 5;
        static event D event1;
        static event D event2;
        private static int F;
        private delegate void D();
        public static int MyProperty { get; set; }

        public static void M(int i) { }

        class Inner
        {
            class SomeName // Noncompliant {{Rename this class to not shadow the outer class' member with the same name.}}
//                ^^^^^^^^
            {
                private int F; // Noncompliant {{Rename this field to not shadow the outer class' member with the same name.}}
//                          ^
            }

            public static int MyProperty { get; set; } //Noncompliant {{Rename this property to not shadow the outer class' member with the same name.}}
            const int a = 7; //Noncompliant
            event D event1; //Noncompliant {{Rename this event to not shadow the outer class' member with the same name.}}
            event D event2 //Noncompliant
            {
                add { }
                remove { }
            }
            private delegate void D(); //Noncompliant {{Rename this delegate to not shadow the outer class' member with the same name.}}

            private int F; //Noncompliant

            public void M(int j) { } // Noncompliant
            public void M1() { }
            public void MyMethod()
            {
                F = 5;
                M(1);
                D delegat = null;
                SomeName delegat2 = null;
                event1();
                F = a;
            }
        }
    }
}
