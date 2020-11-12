using System;

public class MyAttribute : Attribute { }

public record Record
{
    private int field1;
    private string field2;
    private string field3;

    [My()]
    private int fieldWithAttribute;

    public int Compliant
    {
        get { return field1; }
        set { field1 = value; }
    }

    public int Compliant2 //Compliant
    {
        get { return fieldWithAttribute; }
        set { fieldWithAttribute = value; }
    }

    public string PropWithGetAndSet // Noncompliant
    {
        get { return field2; }
        set { field2 = value; }
    }

    public string PropWithGetAndInit // Compliant - FN
    {
        get { return field3; }
        init { field3 = value; }
    }

    public string PropWithGetAndInit { get; init; } // Compliant
}
