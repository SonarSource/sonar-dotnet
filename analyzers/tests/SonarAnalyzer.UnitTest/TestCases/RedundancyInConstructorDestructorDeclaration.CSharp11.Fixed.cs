struct StructWithMulitpleField
{
    public StructWithMulitpleField() { } // Compliant
    public int aField = 42, bField;
}

struct StructWithMulitpleField2
{
    public StructWithMulitpleField2() { } // Compliant
    public int aField, bField = 42;
}

public struct Noncompliant
{

    public bool Foo { get; set; } // No initializer
}
