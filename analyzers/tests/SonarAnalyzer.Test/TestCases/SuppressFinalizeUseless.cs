using System;

namespace Tests.Diagnostics
{
    class Base
    {
        public void M()
        {
            GC.SuppressFinalize(this);
        }
        ~Base()
        { }
    }

    class Derived1 : Base
    {
        public void M()
        {
            GC.SuppressFinalize(this);
        }

        ~Derived1()
        { }
    }
    sealed class Derived2 : Base
    {
        public void M()
        {
            GC.SuppressFinalize(this);
        }
    }

    sealed class C1
    {
        public void M()
        {
            GC.SuppressFinalize(this); //Noncompliant
//          ^^^^^^^^^^^^^^^^^^^^^^^^^
        }
    }

    class C2
    {
        public void M()
        {
            GC.SuppressFinalize(this); // Compliant, not sealed
        }
    }

    class B1 { }
    sealed class C3 : B1
    {
        public void M()
        {
            GC.SuppressFinalize(this); //Noncompliant {{Remove this useless call to 'GC.SuppressFinalize'.}}
        }
    }

    sealed class Dummy
    {
        public void SuppressFinalize(object o)
        { }
        public void M()
        {
            SuppressFinalize(this);
        }
    }

    sealed class Compliant
    {
        ~Compliant()
        { }
        public void M()
        {
            GC.SuppressFinalize(this);
        }
    }

    class NoThis
    {
        public void M()
        {
            GC.SuppressFinalize(new object()); // Compliant - should we raise if `this` is not passed as parameter?
        }
    }
}
