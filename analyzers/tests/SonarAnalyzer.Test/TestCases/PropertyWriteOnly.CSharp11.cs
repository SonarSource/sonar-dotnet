public interface IPropertyWriteOnly
{
    public static virtual int Foo  // Noncompliant {{Provide a getter for 'Foo' or replace the property with a 'SetFoo' method.}}
    {
        set
        {
            // ... some code ...
        }
    }
}
