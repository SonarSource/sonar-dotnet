using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    class MyCompliantClass : IEquatable<MyCompliantClass> // Compliant
    {
        public bool Equals(MyCompliantClass other)
        {
            return false;
        }
    }

    class ClassWithEqualsT // Noncompliant {{Implement 'IEquatable<ClassWithEqualsT>'.}}
//        ^^^^^^^^^^^^^^^^
    {
        public bool Equals(ClassWithEqualsT other)
        {
            return false;
        }
    }

    public class Foo
    {
        protected bool Equals(Foo other)
        {
            return false;
        }
    }
}
