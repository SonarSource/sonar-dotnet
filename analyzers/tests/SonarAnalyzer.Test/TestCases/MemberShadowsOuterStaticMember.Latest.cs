using System;

namespace CSharp9
{
    record OuterRecord
    {
        private delegate void SomeName();
        private static int F;
        public static int MyProperty { get; set; }

        class InnerClass
        {
            class SomeName      // Noncompliant {{Rename this class to not shadow the outer class' member with the same name.}}
            {
                private int F;  // Noncompliant {{Rename this field to not shadow the outer class' member with the same name.}}
                private int InnerOnlyField;
            }

            public static int MyProperty { get; set; }  //Noncompliant {{Rename this property to not shadow the outer class' member with the same name.}}
            public static int InnerOnlyProperty { get; set; }
        }

        record InnerRecord
        {
            class SomeName      // Noncompliant {{Rename this class to not shadow the outer class' member with the same name.}}
            {
                private int F;  // Noncompliant {{Rename this field to not shadow the outer class' member with the same name.}}
                private int InnerOnlyField;
            }

            public static int MyProperty { get; set; }  //Noncompliant {{Rename this property to not shadow the outer class' member with the same name.}}
            public static int InnerOnlyProperty { get; set; }
        }
    }

    class OuterClass
    {
        private delegate void SomeName();
        private static int F;
        public static int MyProperty { get; set; }

        record InnerRecord
        {
            class SomeName      // Noncompliant
            {
                private int F;  // Noncompliant
                private int InnerOnlyField;
            }

            public static int MyProperty { get; set; }  //Noncompliant
            public static int InnerOnlyProperty { get; set; }
        }
    }

    public interface OuterTypes
    {
        public class SomeClass { }
        public interface SomeInterface { }
        public struct SomeStruct { }
        public enum SomeEnum { }

        public delegate void SomeDelegate(int x);

        public interface InnerTypes
        {
            public enum SomeClass { } // Noncompliant
            public struct SomeInterface { } // Noncompliant
            public class SomeStruct { } // Noncompliant
            public enum SomeEnum { } // Noncompliant

            public delegate void SomeDelegate(int x); // Noncompliant
        }
    }
}

namespace CSharp10
{
    record struct OuterRecordStruct
    {
        private delegate void SomeName();
        private static int F;
        public static int MyProperty { get; set; }

        class InnerClass
        {
            record struct SomeName // Noncompliant {{Rename this record struct to not shadow the outer class' member with the same name.}}
            //            ^^^^^^^^
            {
                private int F;     // Noncompliant {{Rename this field to not shadow the outer class' member with the same name.}}
                //          ^
                private int InnerOnlyField;
            }

            public static int MyProperty { get; set; }  // Noncompliant
            public static int InnerOnlyProperty { get; set; }
        }

        record struct InnerRecordStruct
        {
            record SomeName        // Noncompliant {{Rename this record to not shadow the outer class' member with the same name.}}
            {
                private int F;     // Noncompliant
                private int InnerOnlyField;
            }

            public static int MyProperty { get; set; }  // Noncompliant
            public static int InnerOnlyProperty { get; set; }
        }
    }
}

namespace CSharp11
{
    public interface OuterInterface
    {
        public static int StaticFoo1 => 42;
        public static int StaticFoo2 => 42;
        public static int StaticFoo3 => 42;
        public static int StaticFoo4 => 42;
        public static int StaticFoo5 => 42;

        public static int StaticFoo6 => 42;
        public static int StaticFoo7 => 42;

        public static virtual int StaticVirtualFoo1 => 42;
        public static abstract int StaticAbstractFoo1 { get; }
        public const int ConstFoo1 = 42;

        public abstract class InnerClass
        {
            public int StaticFoo1 => 42; // Noncompliant
            public const int StaticFoo2 = 42; // Noncompliant
            public virtual int StaticFoo3 => 42; // Noncompliant
            public abstract int StaticFoo4 { get; } // Noncompliant
            public static int StaticFoo5 => 42; // Noncompliant

            public int StaticVirtualFoo1 => 42; // Compliant, you cannot invoke Interface.Member for virtual members, only on type parameters
            public int StaticAbstractFoo1 => 42; // Compliant, you cannot invoke Interface.Member for abstract members, only on type parameters

            public int ConstFoo1 => 42; // Noncompliant
        }

        public interface InnerInterface
        {
            public int StaticFoo1 => 42; // Noncompliant
            public const int StaticFoo2 = 42; // Noncompliant
            public virtual int StaticFoo3 => 42; // Noncompliant
            public abstract int StaticFoo4 { get; } // Noncompliant
            public static int StaticFoo5 => 42; // Noncompliant

            public static virtual int StaticFoo6 => 42; // Compliant, you cannot invoke Interface.Member for abstract/virtual members, only on type parameters 
            public static abstract int StaticFoo7 { get; } // Compliant, you cannot invoke Interface.Member for abstract/virtual members, only on type parameters

            public int StaticVirtualFoo1 => 42; // Compliant, you cannot invoke Interface.Member for virtual members, only on type parameters
            public int StaticAbstractFoo1 => 42; // Compliant, you cannot invoke Interface.Member for abstract members, only on type parameters

            public int ConstFoo1 => 42; // Noncompliant
        }
    }

    public class OuterClass
    {
        public static int StaticFoo1 => 42;
        public static int StaticFoo2 => 42;
        public static int StaticFoo3 => 42;
        public static int StaticFoo4 => 42;
        public static int StaticFoo5 => 42;

        public static int StaticFoo6 => 42;
        public static int StaticFoo7 => 42;

        public const int ConstFoo1 = 42;

        public interface InnerInterface
        {
            public int StaticFoo1 => 42; // Noncompliant
            public const int StaticFoo2 = 42; // Noncompliant
            public virtual int StaticFoo3 => 42; // Noncompliant
            public abstract int StaticFoo4 { get; } // Noncompliant
            public static int StaticFoo5 => 42; // Noncompliant

            public static virtual int StaticFoo6 => 42; // Compliant, you cannot invoke Interface.Member for abstract/virtual members, only on type parameters 
            public static abstract int StaticFoo7 { get; } // Compliant, you cannot invoke Interface.Member for abstract/virtual members, only on type parameters

            public int ConstFoo1 => 42; // Noncompliant
        }
    }
}

namespace CSharp13
{
    class Outer
    {
        public static int Prop { get; set; }

        partial class Inner
        {
            public partial int Prop { get; } // Noncompliant
        }

        partial class Inner
        {
            public partial int Prop => 42;
        }
    }
}
