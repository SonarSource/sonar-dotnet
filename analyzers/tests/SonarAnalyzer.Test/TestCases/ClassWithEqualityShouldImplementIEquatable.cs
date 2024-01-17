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

        public override bool Equals(object other)
        {
            return false;
        }
    }

    public sealed class Bar : MyEquatable<Bar> // Compliant
    {
        public new bool Equals(Bar other) => false;
    }

    public sealed class FooBar : MyEquatable<Bar> // Compliant - does not define the "Equals<FooBar>" method
    {
        public new bool Equals(Bar other) => false;
    }

    public sealed class BarBar : MyEquatable<Bar> // Noncompliant
    {
        public new bool Equals(Bar other) => false;
        public new bool Equals(BarBar other) => false;
    }

    public sealed class BarFoo : MyEquatable<Bar>, IEquatable2<BarFoo> // Compliant
    {
        public new bool Equals(Bar other) => false;
        public new bool Equals(BarFoo other) => false;
    }

    public abstract class MyEquatable<T> : IEquatable<T>
            where T : MyEquatable<T>
    {
        public bool Equals(T other) => true;
    }

    public interface IEquatable2<T> : IEquatable<T> { }
}
