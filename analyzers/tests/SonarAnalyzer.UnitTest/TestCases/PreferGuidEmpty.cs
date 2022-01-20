using System;

class Compliant
{
    Guid? NullableField; // Compliant
    readonly Guid? ReadonlyNullableField; // Compliant
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
        var instance = new NullableGuidClass(default); // Compliant
        instance.Method(default); // Compliant
    }

    void NotInitiated(string str)
    {
        Guid parsed; // Compliant
        Guid.TryParse(str, out parsed);
    }

    void OptionalParameter(Guid id = default) { } // Compliant, default has to be a run-time constant
}

class NonCompliant
{
    Guid Field; // FN
    readonly Guid ReadonlyField; // FN
    Guid Property { get; set; }

    void DefaultCtor()
    {
        var ctor = new Guid(); // Noncompliant {{Use 'Guid.NewGuid()' or 'Guid.Empty' or add arguments to this GUID instantiation.}}
        //         ^^^^^^^^^^
    }

    void DefaultInintiation()
    {
        Guid defaultValue = default; // Noncompliant
        var defaultOfGuid = default(Guid); // Noncompliant
        Property = default; // Noncompliant
        var instance = new GuidClass(default); // Noncompliant
        instance.Method(default); // Noncompliant
    }

    void EmptyString()
    {
        var emptyCtor = new Guid("00000000-0000-0000-0000-000000000000"); // Noncompliant
        var emptyParse = Guid.Parse("00000000-0000-0000-0000-000000000000"); // FN
    }
}

struct GuidAssignmentStruct
{
    static readonly Guid Static; // FN
    Guid Field; // Compliant, structs do not allow assigned instance values
    readonly Guid ReadOnly; // Compliant, structs do not allow assigned instance values
}
class GuidClass
{
	public GuidClass(Guid param){ }
	public void Method(Guid param){ }
}
class NullableGuidClass
{
	public NullableGuidClass(Guid? param){ }
	public void Method(Guid? param){ }
}
