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

    Guid Field; // Noncompliant
    readonly Guid ReadonlyField; // Noncompliant

    void NotAllowed()
    {
        var ctor = new Guid(); // Noncompliant {{Use 'Guid.NewGuid()' or 'Guid.Empty' or add arguments to this GUID instantiation.}}
        //         ^^^^^^^^^^
        Guid defaultValue = default; // Noncompliant
        var defaultOfGuid = default(Guid); // Noncompliant
        var emptyString = new Guid("00000000-0000-0000-0000-000000000000"); // Noncompliant
        Property = default; // Noncompliant
		var instance = new GuidClass(default); // Noncompliant
		instance.Method(default); // Noncompliant
    }
	
	void Method(Guid? param){ }
	void Method(Guid param){ }
}
struct GuidAssignmentStruct
{
    static readonly Guid Static; // Noncompliant
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