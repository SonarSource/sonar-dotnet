using System;
using System.Collections.Generic;

class Compliant
{
    Guid? nullableField; // Compliant
    readonly Guid? readonlyNullableField; // Compliant
    Guid Property { get; set; } // Compliant

    void Empty()
    {
        var empty = Guid.Empty; // Compliant
    }

    void NotEmpty()
    {
        var rnd = Guid.NewGuid();// Compliant
        var bytes = new Guid(new byte[0]); // Compliant
        var str = new Guid("FA97FFE7-532C-4015-8698-49D8CE4126F4"); // Compliant
    }

    void NullableDefault()
    {
        Guid? nullable = default; // Compliant, not equivalent to Guid.Empty.
        var nullableOfT = default(Guid?); // Compliant
        var guidAsParameter = new NullableGuidClass(default); // Compliant
        guidAsParameter.Method(default); // Compliant
    }

    void NotInitialized(string str)
    {
        Guid parsed; // Compliant
        Guid.TryParse(str, out parsed);
    }

    void OptionalParameter(Guid id = default) { } // Compliant, default has to be a run-time constant

    void NotGuid()
    {
        int integer = default;
        var date = default(DateTime);
    }

    Dictionary<string, string> DoesNotCrashOnDictionaryCreation()
    {
        return new Dictionary<string, string>
        {
            ["a"] = "b"
        };
    }
}

class NonCompliant
{
    Guid field;
    readonly Guid readonlyField;
    Guid Property { get; set; }

    void DefaultCtor()
    {
        var ctor = new Guid(); // Noncompliant {{Use 'Guid.NewGuid()' or 'Guid.Empty' or add arguments to this GUID instantiation.}}
        //         ^^^^^^^^^^
    }

    void DefaultInitialization()
    {
        Guid defaultValue = default; // Noncompliant
        var defaultOfGuid = default(Guid); // Noncompliant
        Property = default; // Noncompliant
        var guidAsParameter = new GuidClass(default); // Noncompliant
        guidAsParameter.Method(default); // Noncompliant
        field = default; // Noncompliant
    }

    void EmptyString()
    {
        var emptyCtor = new Guid("00000000-0000-0000-0000-000000000000"); // Noncompliant
        var emptyParse = Guid.Parse("00000000-0000-0000-0000-000000000000"); // FN
    }
}

struct GuidAssignmentStruct
{
    static readonly Guid Static;
    Guid Field; // Compliant, structs do not allow assigned instance values
    readonly Guid ReadOnly; // Compliant, structs do not allow assigned instance values
}
class GuidClass
{
    public GuidClass(Guid param) { }
    public void Method(Guid param) { }
}
class NullableGuidClass
{
    public NullableGuidClass(Guid? param) { }
    public void Method(Guid? param) { }
}
