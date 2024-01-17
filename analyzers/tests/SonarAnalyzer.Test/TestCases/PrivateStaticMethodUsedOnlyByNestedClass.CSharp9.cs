record OuterRecord
{
    static void UsedOnlyByNestedClass() { }  // Noncompliant
    static void UsedOnlyByNestedRecord() { } // Noncompliant

    class NestedClass
    {
        void Foo()
        {
            UsedOnlyByNestedClass();
        }
    }

    record NestedRecord
    {
        void Foo()
        {
            UsedOnlyByNestedRecord();
        }
    }
}
