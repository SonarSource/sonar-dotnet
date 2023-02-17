
record class EmptyRecordClass1();        // Noncompliant {{Remove this empty record, or add members to it.}}
//           ^^^^^^^^^^^^^^^^^
record class EmptyRecordClass2() { };    // Noncompliant

record struct EmptyRecordStruct1();      // Compliant - this rule only deals with classes
record struct EmptyRecordStruct2() { };

record class NotEmptyRecordClass1(int RecordMember);
record class NotEmptyRecordClass2()
{
    int RecordMember => 42;
};
