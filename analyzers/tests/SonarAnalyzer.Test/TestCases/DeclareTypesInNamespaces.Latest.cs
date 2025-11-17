record PositionalRecord(string FirstParam, string SecondParam); // Noncompliant

record struct RecordStruct              // Noncompliant
{
    record struct InnerRecordStruct;    // Compliant: we want to report only on the outer record
}

record class RecordClass                // Noncompliant
{
    record class InnerRecordClass;      // Compliant: we want to report only on the outer record
}

interface InterfaceWithInnerType        // Noncompliant
{
    interface InnerInterface { }        // Compliant: we want to report only on the outer record
}

static class Extensions                 // Noncompliant
{
    extension(string s) { }             // Compliant: not a type
}

namespace Tests.Diagnostics
{
    record PositionalRecordInNamespace(string FirstParam, string SecondParam);

    record struct RecordStructInNamespace;

    record class RecordClassInNamespace;
}
