namespace Net6;

internal class ExtendedPropertyPatterns
{
    public int[] SomeProperty { get; }

    public void Example(object o)
    {
        if (o is ExtendedPropertyPatterns {SomeProperty.Length: 42 })
        {
            // Do something
        }
    }
}

