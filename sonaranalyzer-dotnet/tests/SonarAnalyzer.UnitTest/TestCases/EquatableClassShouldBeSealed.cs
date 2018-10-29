using System;

namespace Tests.Diagnostics
{
    public class ClassWithVirtualEquals : IEquatable<ClassWithVirtualEquals> // Compliant
    {
        public virtual bool Equals(ClassWithVirtualEquals other) => true;
    }

    public sealed class SealedClassImplementsIEquatable : IEquatable<SealedClassImplementsIEquatable> // Compliant
    {
        public bool Equals(SealedClassImplementsIEquatable other) => true;
    }

    public class ClassImplementsIEquatable : IEquatable<ClassImplementsIEquatable> // Noncompliant {{Seal class 'ClassImplementsIEquatable' or implement 'IEqualityComparer<T>' instead.}}
//               ^^^^^^^^^^^^^^^^^^^^^^^^^
    {
        public bool Equals(ClassImplementsIEquatable other) => true;
    }

    public class ClassProperlyImplementsIEquatable : IEquatable<ClassProperlyImplementsIEquatable> // Noncompliant
    {
        public bool Equals(ClassProperlyImplementsIEquatable other) => true;
    }

    public class ClassHasEqualsMethod // Noncompliant
    {
        public bool Equals(ClassHasEqualsMethod other) => true;
    }

    public class ComplexClass : IEquatable<ComplexClass>, IEquatable<ClassHasEqualsMethod> // Noncompliant
    {
        public virtual bool Equals(ComplexClass other) => true;
        public bool Equals(ClassHasEqualsMethod other) => true;
    }

    public abstract class BaseClass : IEquatable<BaseClass> // Noncompliant
    {
        public bool Equals(BaseClass other) => false;
    }

    public class SubClass : BaseClass { }
    public class SubSubClass : SubClass { }

    public class Foo { }

    public class Bar : Foo, IEquatable<Bar> // Noncompliant
    {
        public bool Equals(Bar other) => false;
    }

    internal class InternalClass : IEquatable<InternalClass> // Compliant because internal
    {
        public bool Equals(InternalClass other) => false;

        private class PrivateClass : IEquatable<PrivateClass> // Compliant because private
        {
            public bool Equals(PrivateClass other) => false;
        }
    }
}
