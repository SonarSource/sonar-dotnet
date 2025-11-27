using System;

namespace Compliant
{
    record RecordWithParameters(int Parameter);

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
    record RecordWithMethodOverride
    {
        public override string ToString() => "";
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

    record NotEmptyGenericRecord<T>(T RecordMember);

    record NotEmptyGenericRecordWithContraint<T>(T RecordMember) where T : class;

    record EmptyRecordProvidingParameterToBase() : RecordWithParameters(42);
}

namespace Noncompliant
{
    record EmptyRecord();                                        // Noncompliant {{Remove this empty record, write its code or make it an "interface".}}
    //     ^^^^^^^^^^^
    record EmptyRecordWithEmptyBody() { };                       // Noncompliant

    record EmptyChildWithoutBrackets : EmptyRecord;              // Noncompliant

    record EmptyChildRecord() : EmptyRecord();                   // Noncompliant

    record EmptyGenericRecord<T>();                              // Noncompliant
    //     ^^^^^^^^^^^^^^^^^^
    record EmptyGenericRecordWithContraint<T>() where T : class; // Noncompliant

    record ConstructorIsPublicAlready : EmptyRecord { }          // Noncompliant

    record BaseRecordWithProtectedConstructor
    {
        protected BaseRecordWithProtectedConstructor() { }
    }

    record WidensConstructorVisibility : BaseRecordWithProtectedConstructor { } // Compliant
}

namespace Ignore
{
    partial record EmptyPartialRecord(); // Compliant - partial classes are ignored, so partial record classes are ignored as well
}

// https://github.com/SonarSource/sonar-dotnet/issues/7709
namespace Repro_7709
{
    interface IMarker { }
    record ImplementsMarker : IMarker { }
    record ImplementsEmptyRecordAndMarker : Noncompliant.EmptyRecord, IMarker { }
}

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

namespace Compliant
{
    class ChildClass() : BaseClass(42) { } // Compliant

    class ChildClassWithParameters(int value) : BaseClass(value) { } // Compliant

    class BaseClass(int value) { }
}

namespace Noncompliant
{
    class ChildClass() : BaseClass() { } // Noncompliant

    class BaseClass()
    {
        public int Value { get; init; }
    }
}

public partial class PartialConstructor
{
    public partial PartialConstructor();
}

public partial class PartialEvent
{
    public partial event EventHandler MyEvent;
}
