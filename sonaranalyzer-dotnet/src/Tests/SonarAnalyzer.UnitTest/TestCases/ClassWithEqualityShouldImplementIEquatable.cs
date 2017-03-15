using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    class MyCompliantClass : IEquatable<MyCompliantClass> // Compliant
    {
        public override bool Equals(object obj)
        {
            return Equals(obj as MyCompliantClass);
        }

        public bool Equals(MyCompliantClass other)
        {
            return false;
        }
    }

    class ClassWithEqualsObj // Noncompliant {{Implement 'IEquatable<ClassWithEqualsObj>'.}}
//        ^^^^^^^^^^^^^^^^^^
    {
        public override bool Equals(object obj)
//                           ^^^^^^ Secondary {{Call 'Equals(T)' from this method.}}
        {
            return base.Equals(obj);
        }
    }

    class ClassWithEqualsT // Noncompliant {{Implement 'IEquatable<ClassWithEqualsT>' and override 'Equals(object)'.}}
//        ^^^^^^^^^^^^^^^^
    {
        public bool Equals(ClassWithEqualsT other)
//                  ^^^^^^ Secondary {{Call this method from 'Equals(object)'.}}
        {
            //...
        }
    }

    class ClassWithEquatableOnly : IEquatable<ClassWithEquatableOnly> // Noncompliant {{Override 'Equals(object)'.}}
    {
        public bool Equals(ClassWithEquatableOnly other)
//                  ^^^^^^ Secondary {{Call this method from 'Equals(object)'.}}
        {
            return false;
        }
    }

    class ClassWithoutEquatable // Noncompliant {{Implement 'IEquatable<ClassWithoutEquatable>'.}}
    {
        public override bool Equals(object obj)
        {
            return Equals(obj as ClassWithoutEquatable);
        }

        public bool Equals(ClassWithoutEquatable other)
        {
            return false;
        }
    }

    interface IFoo : IEquatable<IFoo> { }
    abstract class Foo : IFoo // Noncompliant
    {
        public bool Equals(IFoo other) // Secondary
        {
            return true;
        }
    }
    abstract class Foo2 : IFoo // Compliant
    {
        public sealed override bool Equals(object obj)
        {
            return Equals(obj as IFoo);
        }
        public abstract bool Equals(IFoo other);
    }

    class FooBar : Foo2 // Compliant
    {
        public override bool Equals(IFoo other)
        {
            throw new NotImplementedException();
        }
    }

    abstract class Foo3 : IFoo // Compliant
    {
        public override bool Equals(object obj)
        {
            return Equals(obj as IFoo);
        }
        public abstract bool Equals(IFoo other);
    }

    class FooBar3 : Foo3 // Compliant
    {
        public override bool Equals(IFoo other)
        {
            throw new NotImplementedException();
        }
    }

    class MyClass : IFoo // Compliant
    {
        public override bool Equals(object obj)
        {
            return Equals(obj as IFoo);
        }
        public bool Equals(IFoo other)
        {
            return true;
        }
    }

    class SubClass : MyClass // Compliant
    {

    }
}