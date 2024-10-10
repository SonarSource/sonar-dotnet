using System;

namespace CSharp13
{
    public partial class PartialProperties
    {
        public partial int MyProperty
        {
            get
            {
                var x = 5;
                throw new NotSupportedException(); // Compliant
            }
        }
        public partial int MyProperty2
        {
            get
            {
                throw new NotImplementedException(); // Compliant
            }
        }
        public partial int MyProperty3
        {
            get
            {
                throw new PlatformNotSupportedException(); // Compliant
            }
        }
        public partial int MyProperty4
        {
            get
            {
                throw new Exception(); // Noncompliant {{Remove the exception throwing from this property getter, or refactor the property into a method.}}
    //          ^^^^^^^^^^^^^^^^^^^^^^
            }

            set
            {
                throw new Exception(); // Compliant
            }
        }
    }
}
