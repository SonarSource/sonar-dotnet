using System;

record Record
{
    public void Foo<T1, T2, T3>() { }
    public void Foo<T1, T2, T3, T4>() { } // Noncompliant  {{Reduce the number of generic parameters in the 'Record.Foo' method to no more than the 3 authorized.}}
//              ^^^
}

record PositionalRecord(int SomeProperty)
{
    public void Foo<T1, T2, T3>() { }
    public void Foo<T1, T2, T3, T4>() { } // Noncompliant  {{Reduce the number of generic parameters in the 'PositionalRecord.Foo' method to no more than the 3 authorized.}}
//              ^^^
}

record Record<T1, T2> { }
record Record<T1, T2, T3> { } // Noncompliant {{Reduce the number of generic parameters in the 'Record' record to no more than the 2 authorized.}}

record PositionalRecord<T1, T2>(int SomeProperty) { }
record PositionalRecord<T1, T2, T3>(int SomeProperty) { } // Noncompliant {{Reduce the number of generic parameters in the 'PositionalRecord' record to no more than the 2 authorized.}}

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

class SomeAttribute<T1, T2, T3, T4> : Attribute // Noncompliant
{
}

class Bar<T1, T2, T3>(string classParam) // Noncompliant
{
    void Method()
    {
        bool GenericLambda<T1, T2, T3, T4>(T1 lambdaParam = default(T1)) => true; // Noncompliant
    }
}
