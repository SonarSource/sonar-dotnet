using System;

class GuidAssignment
{
    Guid? NullableField; // Compliant
    readonly Guid? ReadonlyNullableField; // Compliant
    Guid Property { get; set; } // Compliant

    void Allowed()
    {
        var empty = Guid.Empty; // Compliant
        var rnd = Guid.NewGuid(); // Compliant
        var bytes = new Guid(new byte[0]); // Compliant
        var str = new Guid("FA97FFE7-532C-4015-8698-49D8CE4126F4"); // Compliant
        Guid? nullable = default; // Compliant, not equivalent to Guid.Empty.
        var nullableOfT = default(Guid?); // Compliant
        var instance = new NullableGuidClass(default); // Compliant
        instance.Method(default); // Compliant
        Guid parsed; // Compliant
        Guid.TryParse("FA97FFE7-532C-4015-8698-49D8CE4126F4", out parsed);
    }

    Guid Field; // Fixed
    readonly Guid ReadonlyField; // Fixed

    void NotAllowed()
    {
        var ctor = Guid.Empty; // Fixed
        Guid defaultValue = Guid.Empty; // Fixed
        var defaultOfGuid = Guid.Empty; // Fixed
        var emptyString = Guid.Empty; // Fixed
        Property = Guid.Empty; // Fixed
        var instance = new GuidClass(Guid.Empty); // Fixed
        instance.Method(Guid.Empty); // Fixed
    }

    void Method(Guid? param) { }
    void Method(Guid param) { }
}
struct GuidAssignmentStruct
{
    static readonly Guid Static; // Fixed
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
