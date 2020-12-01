using System;

public class MyAttribute : Attribute { }

public record Record
{
    private string field1;
    private string field2;

    [My()]
    private int fieldWithAttribute;
    public int Compliant
    {
        get { return fieldWithAttribute; }
        set { fieldWithAttribute = value; }
    }

    public string PropWithGetAndSet // Noncompliant
    {
        get { return field1; }
        set { field1 = value; }
    }

    public string PropWithGetAndInit // Compliant - FN
    {
        get { return field2; }
        init { field2 = value; }
    }

    public string AutoPropWithGetAndInit { get; init; } // Compliant
}
