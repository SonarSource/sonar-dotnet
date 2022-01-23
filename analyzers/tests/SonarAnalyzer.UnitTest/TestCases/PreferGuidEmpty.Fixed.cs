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

    void NotGuid()
    {
        int integer = default;
        var date = default(DateTime);
    }
}

class NonCompliant
{
    Guid Field; // FN
    readonly Guid ReadonlyField; // FN
    Guid Property { get; set; }

    void DefaultCtor()
    {
        var ctor = Guid.Empty; // Fixed
    }

    void DefaultInintiation()
    {
        Guid defaultValue = Guid.Empty; // Fixed
        var defaultOfGuid = Guid.Empty; // Fixed
        Property = Guid.Empty; // Fixed
        var instance = new GuidClass(Guid.Empty); // Fixed
        instance.Method(Guid.Empty); // Fixed
    }

    void EmptyString()
    {
        var emptyCtor = Guid.Empty; // Fixed
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
    public GuidClass(Guid param) { }
    public void Method(Guid param) { }
}
class NullableGuidClass
{
    public NullableGuidClass(Guid? param) { }
    public void Method(Guid? param) { }
}
