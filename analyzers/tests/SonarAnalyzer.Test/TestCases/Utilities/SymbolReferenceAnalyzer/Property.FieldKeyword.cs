public class Sample
{
    public int Property { get { return field; } set { field = value; } }

    public int property { get; set; }

    public void Go()
    {
        var x = Property;
        Property = 42;
        property = 24;
    }
}
