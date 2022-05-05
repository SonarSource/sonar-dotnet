record struct RecordStruct
{
    public void Foo<T1, T2, T3>() { }
    public void Foo<T1, T2, T3, T4>() { } // Noncompliant  {{Reduce the number of generic parameters in the 'RecordStruct.Foo' method to no more than the 3 authorized.}}
    //          ^^^
}

record struct PositionalRecordStruct(int SomeProperty)
{
    public void Foo<T1, T2, T3>() { }
    public void Foo<T1, T2, T3, T4>() { } // Noncompliant  {{Reduce the number of generic parameters in the 'PositionalRecordStruct.Foo' method to no more than the 3 authorized.}}
}

record struct RecordStruct<T1, T2> { }
record struct RecordStruct<T1, T2, T3> { } // Noncompliant  {{Reduce the number of generic parameters in the 'RecordStruct' record struct to no more than the 2 authorized.}}
//            ^^^^^^^^^^^^

record struct PositionalRecordStruct<T1, T2>(int SomeProperty) { }
record struct PositionalRecordStruct<T1, T2, T3>(int SomeProperty) { } // Noncompliant  {{Reduce the number of generic parameters in the 'PositionalRecordStruct' record struct to no more than the 2 authorized.}}
