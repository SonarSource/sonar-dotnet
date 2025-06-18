using System;

public class MyAttribute : Attribute { }

public unsafe record Record
{
    private string field1;
    private string field2;
    private int prop;
    private Coord* coord1; // Error [CS8908]
    private Coord coord2;
    private Record r;

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

    public string PropWithGetAndInit // Noncompliant
    {
        get { return field2; }
        init { field2 = value; }
    }

    public string AutoPropWithGetAndInit { get; init; } // Compliant

    public int Prop1 { get; get; } // Error [CS1007]

    public int Prop2
    {
        get
        {
            return prop;
        }

        set
        {
            prop += 1;
        }
    }

    public int Prop3
    {
        get
        {
            return coord1->X;
        }
        set
        {
            prop += 1;
        }
    }

    public int Prop4
    {
        get
        {
            return r.coord2.X;
        }
        set
        {
            prop = value;
        }
    }
}

public struct Coord
{
    public int X;
}

namespace CSharp13
{
    // https://sonarsource.atlassian.net/browse/NET-456
    public partial class PartialProperties
    {
        private string field;
        private int[] array = new int[100];

        public partial string Prop // Compliant
        {
            get { return field; }
            set { field = value; }
        }

        public partial int this[int index]
        {
            get { return array[index]; }
            set { array[index] = value; }
        }
    }

    public partial class PartialProperties
    {
        public partial string Prop { get; set; }

        public partial int this[int index] { get; set; }
    }
}
