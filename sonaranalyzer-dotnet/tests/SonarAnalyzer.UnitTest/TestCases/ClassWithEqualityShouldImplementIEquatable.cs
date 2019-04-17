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

    public sealed class Bar : MyEquatable<Bar> // Noncompliant FP - issue #1960
    {
        public Bar(string x)
        {
            X = x;
        }

        public string X { get; }

        public new bool Equals(Bar other) => string.Equals(X, other.X, StringComparison.OrdinalIgnoreCase);
    }

    public abstract class MyEquatable<T> : IEquatable<T>
            where T : MyEquatable<T>
    {
        public bool Equals(T other) => true;
    }
}
