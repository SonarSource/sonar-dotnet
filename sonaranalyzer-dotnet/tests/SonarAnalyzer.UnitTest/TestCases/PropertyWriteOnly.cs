namespace Tests.Diagnostics
{
    public class PropertyWriteOnly
    {
        public int Foo  //Noncompliant {{Provide a getter for 'Foo' or replace the property with a 'SetFoo' method.}}
//                 ^^^
        {
            set
            {
                // ... some code ...
            }
        }
        public int Foo2
        {
            get
            {
                return 1;
            }
            set
            {
                // ... some code ...
            }
        }
    }
}
