
record class EmptyRecordClass1();        // Noncompliant
//           ^^^^^^^^^^^^^^^^^
record class EmptyRecordClass2() { };    // Noncompliant

record struct EmptyRecordStruct1();      // Compliant - this rule only deals with classes
record struct EmptyRecordStruct2() { };

record class NotEmptyRecordClass1(int RecordMember);
record class NotEmptyRecordClass2()
{
    int RecordMember => 0;
};

record struct NotEmptyRecordStruct1(int RecordMember);
record struct NotEmptyRecordStruct2()
{
    int RecordMember => 0;
};
