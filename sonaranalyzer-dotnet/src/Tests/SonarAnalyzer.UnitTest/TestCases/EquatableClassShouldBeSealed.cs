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

    public static class StaticClassImplementsIEquatable : IEquatable<StaticClassImplementsIEquatable> // Compliant
    {
        public bool Equals(StaticClassImplementsIEquatable other) => true;
    }

    public class ClassImplementsIEquatable : IEquatable<ClassImplementsIEquatable> // Noncompliant {{Make this class 'sealed' or implement 'IEqualityComparer<T>' instead.}}
//               ^^^^^^^^^^^^^^^^^^^^^^^^^
    {
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
}
