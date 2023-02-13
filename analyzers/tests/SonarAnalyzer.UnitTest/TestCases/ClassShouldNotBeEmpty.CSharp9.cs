
record EmptyRecord1();                      // Noncompliant
//     ^^^^^^^^^^^^
record EmptyRecord2() { };                  // Noncompliant

record NotEmptyRecord1(int RecordMember);
record NotEmptyRecord2()
{
    int RecordMember => 0;
};
