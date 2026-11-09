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

static class ExtensionsWithNestedType   // Noncompliant
{
    extension(string s)
    {
        class InnerClassOfExtensionBlock { }    // Error [CS9282]
    }
}

namespace Tests.Diagnostics
{
    record PositionalRecordInNamespace(string FirstParam, string SecondParam);

    record struct RecordStructInNamespace;

    record class RecordClassInNamespace;

    union UnionInNamespace(int, string)
    {
        class InnerClassOfUnion { }             // Compliant NET-4916
        enum InnerEnumOfUnion { }               // Compliant
        struct InnerStructOfUnion { }           // Compliant
        interface IInnerOfUnion { }             // Compliant
        record InnerRecordOfUnion;              // Compliant
        record struct InnerRecordStructOfUnion; // Compliant
    }

    closed class ClosedInNamespace;
}

union GlobalUnion(int, string);         // FN NET-4915

union GlobalUnionWithNestedType(int, string)    // FN NET-4915
{
    class InnerClassOfGlobalUnion { }           // Compliant NET-4916: we want to report only on the outer union (FN NET-4915)
}

closed class GlobalClosed;              // Noncompliant

class GlobalClassWithNestedUnion        // Noncompliant
{
    union NestedUnion(int, string);
}

class GlobalClassWithNestedUnionWithNestedType  // Noncompliant
{
    union NestedUnion(int, string)
    {
        class InnerClassOfNestedUnion { }       // Compliant
    }
}

[System.Runtime.CompilerServices.Union]
struct GlobalHandWrittenUnion           // Noncompliant
{
    public GlobalHandWrittenUnion(int value) => Value = value;
    public object Value { get; }
}
