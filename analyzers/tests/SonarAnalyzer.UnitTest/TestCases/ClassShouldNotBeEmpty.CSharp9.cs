using System;

record EmptyRecord1();                                              // Noncompliant {{Remove this empty record, or add members to it.}}
//     ^^^^^^^^^^^^
record EmptyRecord2() { };                                          // Noncompliant

record RecordWithParameters(int RecordMember);

record RecordWithProperty
{
    int SomeProperty => 42;
}
record RecordWithField
{
    int SomeField = 42;
}
record RecordWithMethod
{
    void Method() { }
}
record RecordWithIndexer
{
    int this[int index] => 42;
}
record RecordWithDelegate
{
    delegate void MethodDelegate();
}
record RecordWithEvent
{
    event EventHandler CustomEvent;
}

record EmptyGenericRecord<T>();                                     // Noncompliant
//     ^^^^^^^^^^^^^^^^^^
record EmptyGenericRecordWithContraint<T>() where T : class;        // Noncompliant
record NotEmptyGenericRecord<T>(T RecordMember);
record NotEmptyGenericRecordWithContraint<T>(T RecordMember) where T : class;
