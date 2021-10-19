using System;

record struct OuterRecordStruct
{
    private delegate void SomeName();
    private static int F;
    public static int MyProperty { get; set; }

    class InnerClass
    {
        class SomeName      // FN
        {
            private int F;  // FN
            private int InnerOnlyField;
        }

        public static int MyProperty { get; set; }  // FN
        public static int InnerOnlyProperty { get; set; }
    }

    record struct InnerRecordStruct
    {
        class SomeName      // FN
        {
            private int F;  // FN
            private int InnerOnlyField;
        }

        public static int MyProperty { get; set; }  // FN
        public static int InnerOnlyProperty { get; set; }
    }
}
