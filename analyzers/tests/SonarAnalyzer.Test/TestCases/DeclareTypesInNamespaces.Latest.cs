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

    union UnionInNamespace(int, string)
    {
        class InnerClassOfUnion { }     // Noncompliant {{Move 'InnerClassOfUnion' into a named namespace.}} FP NET-4916
    }

    closed class ClosedInNamespace;
}

union GlobalUnion(int, string);         // FN NET-4915

union GlobalUnionWithNestedType(int, string)    // FN NET-4915
{
    class InnerClassOfGlobalUnion { }           // Noncompliant {{Move 'InnerClassOfGlobalUnion' into a named namespace.}} FP NET-4916
}

closed class GlobalClosed;              // Noncompliant

class GlobalClassWithNestedUnion        // Noncompliant
{
    union NestedUnion(int, string);
}

[System.Runtime.CompilerServices.Union]
struct GlobalHandWrittenUnion           // Noncompliant
{
    public GlobalHandWrittenUnion(int value) => Value = value;
    public object Value { get; }
}
