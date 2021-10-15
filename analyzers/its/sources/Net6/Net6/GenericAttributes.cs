namespace Net6
{
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
}
