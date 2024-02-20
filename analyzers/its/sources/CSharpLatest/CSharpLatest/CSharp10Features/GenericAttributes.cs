namespace CSharpLatest.CSharp10Features;

// Generic Attributes
public class MyAttribute<T> : Attribute
{
    public MyAttribute(string s)
    {
    }
}

internal class GenericAttributeUsage
{
    [MyAttribute<int>("")]
    public void Example()
    {

    }
}
