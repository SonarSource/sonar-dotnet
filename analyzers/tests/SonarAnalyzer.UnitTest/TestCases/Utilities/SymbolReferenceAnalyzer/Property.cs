public class Sample
{
    public int Property { get; set; }

    public void Go()
    {
        var x = Property;
        Property = 42;
    }
}
