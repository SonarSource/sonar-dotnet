namespace Tests.Diagnostics
{
    public class PropertyGetterWithThrow
    {
        public int MyProperty
        {
            get
            {
                var x = 5;
                throw new System.NotSupportedException(); //Noncompliant {{Remove the exception throwing from this property getter, or refactor the property into a method.}}
//              ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            }
            set { }
        }
        public int this[int i]
        {
            get
            {
                throw new System.Exception(); // okay
            }
            set
            {
            }
        }
    }
}
