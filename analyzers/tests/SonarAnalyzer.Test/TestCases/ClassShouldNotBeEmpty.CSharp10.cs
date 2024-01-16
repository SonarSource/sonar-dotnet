namespace Compliant
{
    record class NotEmptyRecordClass1(int RecordMember);
    record class NotEmptyRecordClass2()
    {
        int RecordMember => 42;
    };
}
namespace NonCompliant
{

    record class EmptyRecordClass();                           // Noncompliant {{Remove this empty record, write its code or make it an "interface".}}
    //           ^^^^^^^^^^^^^^^^
    record class EmptyRecordClassWithEmptyBody() { };          // Noncompliant

    record class EmptyChildWithoutBrackets : EmptyRecordClass; // Noncompliant
}
namespace Ignored
{
    record struct EmptyRecordStruct();                  // Compliant - this rule only deals with classes
    record struct EmptyRecordStructWithEmptyBody() { }; // Compliant - this rule only deals with classes
}
