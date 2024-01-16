using System;

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
