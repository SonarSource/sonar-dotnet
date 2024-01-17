void LocalFoo<T1, T2, T3>() { }
void LocalBar<T1, T2, T3, T4>() { } // Noncompliant {{Reduce the number of generic parameters in the 'LocalBar' method to no more than the 3 authorized.}}

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
