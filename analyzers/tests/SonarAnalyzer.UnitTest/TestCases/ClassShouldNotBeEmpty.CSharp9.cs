
record EmptyRecord1();                                              // Noncompliant {{Remove this empty record, or add members to it.}}
//     ^^^^^^^^^^^^
record EmptyRecord2() { };                                          // Noncompliant

record NotEmptyRecord1(int RecordMember);
record NotEmptyRecord2()
{
    int RecordMember => 42;
};

record EmptyGenericRecord<T>();                                     // Noncompliant
//     ^^^^^^^^^^^^^^^^^^
public record EmptyGenericRecordWithContraint<T>() where T : class; // Noncompliant
record NotEmptyGenericRecord<T>(T RecordMember);
public record NotEmptyGenericRecordWithContraint<T>(T RecordMember) where T : class;
