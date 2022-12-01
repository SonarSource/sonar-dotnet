using System;

namespace Tests.TestCases
{
    class MemberShadowsOuterStaticMember
    {
        private delegate void SomeName();

        const int a = 5;
        static event D event1;
        static event D event2;
        private static int F1;
        private int F2;
        private delegate void D();
        public static int MyProperty { get; set; }

        public static void M(int i) { }

        class Inner
        {
            class SomeName // Noncompliant {{Rename this class to not shadow the outer class' member with the same name.}}
            //    ^^^^^^^^
            {
                private int F1; // Noncompliant {{Rename this field to not shadow the outer class' member with the same name.}}
                //          ^^
                private int F2; // Compliant: Non static.
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

            private int F1; //Noncompliant

            public void M(int j) { } // Noncompliant
            public void M1() { }
            public void MyMethod()
            {
                F1 = 5;
                M(1);
                D delegat = null;
                SomeName delegat2 = null;
                event1();
                F1 = a;
            }
        }
    }

    struct OuterStruct
    {
        private delegate void SomeName();
        private static int F;
        public static int MyProperty { get; set; }
        public enum SomeEnum { }

        class InnerClass
        {
            class SomeName      // Noncompliant {{Rename this class to not shadow the outer class' member with the same name.}}
            //    ^^^^^^^^
            {
                private int F;  // Noncompliant {{Rename this field to not shadow the outer class' member with the same name.}}
                //          ^
                private int InnerOnlyField;
            }

            public static int MyProperty { get; set; }  // Noncompliant
            public static int InnerOnlyProperty { get; set; }
        }

        struct InnerStruct1 // Don't rename. It is used as a collision target
        {
            class SomeName      // Noncompliant
            {
                private int F;  // Noncompliant
                private int InnerOnlyField;
            }

            public static int MyProperty { get; set; }  // Noncompliant
            public static int InnerOnlyProperty { get; set; }
        }

        struct InnerStruct2
        {
            struct SomeName       // Noncompliant
            {
            }
        }

        struct InnerStruct3
        {
            struct InnerStruct1   // Noncompliant
            {
            }
        }

        struct InnerStruct4
        {
            enum InnerStruct1 { } // Noncompliant
        }

        struct InnerStruct5
        {
            struct SomeEnum { }   // Noncompliant
        }
    }

    public class SomeEnumContainer 
    {
        public enum SomeEnum
        {
            Value1, // Compliant
            Value2, // Compliant
        }

        public static int Value1 = 42;
        public const int Value2 = 43;
    }
}
