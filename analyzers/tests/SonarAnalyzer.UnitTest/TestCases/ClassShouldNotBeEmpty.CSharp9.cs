
record EmptyRecord1();                      // Noncompliant {{Remove this empty record, or add members to it.}}
//     ^^^^^^^^^^^^
record EmptyRecord2() { };                  // Noncompliant

record NotEmptyRecord1(int RecordMember);
record NotEmptyRecord2()
{
    int RecordMember => 0;
};
