public partial class Sample
{
    private int field;

    public int Prop1
    {
        get
        {
            int field = 0; // Noncompliant {{'field' is a contextual keyword in C# 14. Rename it, escape it as '@field', or qualify member access as 'this.field' to avoid ambiguity.}}
            return 0;
        }
    }

    public int Prop2
    {
        get
        {
            return field; // Noncompliant
        }
    }
}
