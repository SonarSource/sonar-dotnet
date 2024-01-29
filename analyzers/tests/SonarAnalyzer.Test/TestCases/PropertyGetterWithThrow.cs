namespace Tests.Diagnostics
{
    using System;

    public class PropertyGetterWithThrow
    {
        public int MyProperty
        {
            get
            {
                var x = 5;
                throw new NotSupportedException(); // Compliant
            }
        }
        public int MyProperty2
        {
            get
            {
                throw new NotImplementedException(); // Compliant
            }
        }
        public int MyProperty3
        {
            get
            {
                throw new PlatformNotSupportedException(); // Compliant
            }
        }
        public int MyProperty4
        {
            get
            {
                throw new Exception(); // Noncompliant {{Remove the exception throwing from this property getter, or refactor the property into a method.}}
//              ^^^^^^^^^^^^^^^^^^^^^^
            }
        }
        public int MyProperty5
        {
            get
            {
                return 42;
            }
            set
            {
                throw new Exception(); // Compliant - setters are ignored by this rule
            }
        }
        public int MyProperty6
        {
            get
            {
                throw new InvalidOperationException(); // Compliant
            }
        }
        public int MyProperty7
        {
            get
            {
                throw new ObjectDisposedException(""); // Compliant
            }
        }
        public int MyProperty8
        {
            get
            {
                try
                {
                }
                catch(Exception)
                {
                    throw; // Compliant
                }

                return 0;
            }
        }
        public int this[int i]
        {
            get
            {
                throw new System.Exception(); // Compliant - indexed getters are ignored by this rule
            }
            set
            {
            }
        }
        public int MyProperty9
        {
            get
            {
                throw FactoryMethod(); // Compliant
            }
        }
        private NotSupportedException FactoryMethod()
        {
            return new NotSupportedException();
        }
        public int MyProperty10
        {
            get
            {
                throw FactoryMethod2(); // Noncompliant {{Remove the exception throwing from this property getter, or refactor the property into a method.}}
//              ^^^^^^^^^^^^^^^^^^^^^^^
            }
        }
        private Exception FactoryMethod2()
        {
            return new Exception();
        }
    }
}
