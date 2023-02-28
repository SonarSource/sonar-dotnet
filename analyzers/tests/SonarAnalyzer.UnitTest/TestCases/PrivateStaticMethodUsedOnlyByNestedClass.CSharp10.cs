record class OuterRecordClass
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

    record class NestedRecord
    {
        void Foo()
        {
            UsedOnlyByNestedRecord();
        }
    }
}

record struct OuterRecordStruct
{
    private static void UsedOnlyByNestedClass() { }  // Noncompliant
    private static void UsedOnlyByNestedRecord() { } // Noncompliant

    class NestedClass
    {
        void Foo()
        {
            UsedOnlyByNestedClass();
        }
    }

    record struct NestedRecord
    {
        void Foo()
        {
            UsedOnlyByNestedRecord();
        }
    }
}
