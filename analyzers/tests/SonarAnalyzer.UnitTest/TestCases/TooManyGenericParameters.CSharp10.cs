void LocalFoo<T1, T2, T3>() { }
void LocalBar<T1, T2, T3, T4>() { } // Compliant - FN

record struct RecordStruct
{
    public void Foo<T1, T2, T3>() { }
    public void Foo<T1, T2, T3, T4>() { } // Noncompliant  {{Reduce the number of generic parameters in the '.Foo' method to no more than the 3 authorized.}}
                                          // Although an issue is raised, the above message is not correct. There should be 'RecordStruct.Foo' instead of '.Foo'.
}

record struct PositionalRecordStruct(int SomeProperty)
{
    public void Foo<T1, T2, T3>() { }
    public void Foo<T1, T2, T3, T4>() { } // Noncompliant  {{Reduce the number of generic parameters in the '.Foo' method to no more than the 3 authorized.}}
                                          // Although an issue is raised, the above message is not correct. There should be 'PositionalRecordStruct.Foo' instead of '.Foo'.
}

record struct RecordStruct<T1, T2> { }
record struct RecordStruct<T1, T2, T3> { } // FN

record struct PositionalRecordStruct<T1, T2>(int SomeProperty) { }
record struct PositionalRecordStruct<T1, T2, T3>(int SomeProperty) { } // FN
