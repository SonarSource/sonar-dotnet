public partial class Sample
{
    public int Prop3
    {
        get
        {
            return field; // Noncompliant
        }
    }
}
