using System;
using System.Runtime.CompilerServices;

namespace Tests.CustomType
{
    public class ModuleInitializerAttribute : Attribute { }

    public class Foo
    {
        [ModuleInitializer]
        internal static void M1()
        {
            throw new Exception(); // Compliant - the attribute is not in the System.Runtime.CompilerServices namespace
        }
    }
}

record Record1 : IDisposable
{
    static Record1() => throw new NotImplementedException(); // Compliant (allowed exception)
    public void Dispose()
    {
        throw new NotImplementedException(); // Compliant (allowed exception)
    }
}

record Record2 : IDisposable
{
    static Record2() => throw new Exception(); // Noncompliant {{Remove this 'throw' expression.}}
//                      ^^^^^^^^^^^^^^^^^^^^^

    [ModuleInitializer]
    internal static void M1()
    {
        throw new Exception(); // Noncompliant
    }

    [Obsolete]
    [System.Runtime.CompilerServices.ModuleInitializerAttribute]
    internal static void M2() => throw new Exception(); // Noncompliant

    [Obsolete]
    internal static void M3() => throw new Exception(); // Compliant - different attribute

    [Obsolete("This attribute has arguments.", true)]
    internal static void M4() => throw new Exception(); // Compliant - different attribute

    event EventHandler OnSomething
    {
        add
        {
            throw new Exception(); // Noncompliant
        }
        remove
        {
            throw new InvalidOperationException(); // Compliant - allowed exception
        }
    }

    event EventHandler OnSomeArrow
    {
        add => throw new Exception(); // Noncompliant
        remove => throw new InvalidOperationException(); // Compliant - allowed exception
    }

    public override string ToString()
    {
        throw new Exception(); // Noncompliant
    }

    public void Dispose()
    {
        throw new Exception(); // Noncompliant
    }
}

interface IMyInterface
{
    static virtual event EventHandler VirtualEvent
    {
        add => throw new Exception(); // Noncompliant
        remove => throw new InvalidOperationException(); // Compliant - allowed exception
    }

    static abstract event EventHandler AbstractEvent;
}

class MyClass : IMyInterface
{
    public static event EventHandler AbstractEvent
    {
        add => throw new Exception(); // Noncompliant
        remove => throw new InvalidOperationException(); // Compliant - allowed exception
    }
}

partial class PartialClass
{
    partial PartialClass() => throw new Exception();    // Compliant

    partial event EventHandler PartialEvent
    {
        add => throw new Exception();                   // Noncompliant
        remove { throw new Exception(); }               // Noncompliant
    }

    static partial event EventHandler StaticPartialEvent
    {
        add { throw new Exception(); }                  // Noncompliant
        remove => throw new Exception();                // Noncompliant
    }
}

namespace Extensions
{
    class Sample { }

    static class Extensions
    {
        extension(Sample)
        {
            public static bool operator ==(Sample a, Sample b)
            {
                throw new Exception(); // Noncompliant
            }

            public static bool operator !=(Sample a, Sample b)
            {
                throw new Exception(); // Noncompliant
            }
        }

        extension(Sample s)
        {
            public bool Equals(object obj) => throw new Exception();    // Compliant NET-2707
            public int GetHashCode() => throw new Exception();          // Compliant
            public string ToString() { throw new Exception(); }         // Compliant
            public void Dispose() { throw new Exception(); }            // Compliant
        }
    }
}

namespace CSharp15
{
    public class Cat { }
    public class Dog { }

    [System.Runtime.CompilerServices.Union]
    public struct Pet : System.Runtime.CompilerServices.IUnion
    {
        public Pet(Cat value) => Value = value;
        public Pet(Dog value) => throw new Exception(); // Noncompliant NET-4999
        public object Value { get; }
    }

    [System.Runtime.CompilerServices.Union]
    public class PetWithHelpers : System.Runtime.CompilerServices.IUnion
    {
        public PetWithHelpers(Cat value) => Value = value;
        public PetWithHelpers(Dog value) => throw new NotImplementedException(); // Compliant, allowed exception
        private PetWithHelpers(int value) => throw new Exception();              // Compliant, not a creation member
        internal PetWithHelpers(string value) => throw new Exception();          // Compliant, not a creation member
        public PetWithHelpers(Cat cat, Dog dog) => throw new Exception();        // Compliant, two parameters
        public object Value { get; }
    }

    public class NotAUnion
    {
        public NotAUnion(Cat value) => throw new Exception();                    // Compliant, not a union
    }

    [System.Runtime.CompilerServices.Union]
    public struct PetWithGuard : System.Runtime.CompilerServices.IUnion
    {
        public PetWithGuard(Cat value) => Value = value ?? throw new ArgumentNullException(nameof(value)); // Noncompliant, creation members only allow NotImplementedException
        public object Value { get; }
    }

    [System.Runtime.CompilerServices.Union]
    public struct PetWithProvider : System.Runtime.CompilerServices.IUnion, PetWithProvider.IUnionMembers
    {
        public interface IUnionMembers
        {
            static abstract PetWithProvider Create(Cat value);
            object Value { get; }
        }
        private PetWithProvider(Cat value) => throw new Exception();                  // Compliant, conversions call Create
        public static PetWithProvider Create(Cat value) => new PetWithProvider(value);
        public object Value { get; }
    }

    [System.Runtime.CompilerServices.Union]
    public struct PetWithUnimplementedMembers : System.Runtime.CompilerServices.IUnion
    {
        public interface IUnionMembers
        {
            static PetWithUnimplementedMembers Create(Cat value) => default;
        }
        public PetWithUnimplementedMembers(Cat value) => throw new Exception();       // Noncompliant, IUnionMembers is not implemented, so constructors are the creation members
        public object Value { get; }
    }

    public class UnionUsage
    {
        public void Convert()
        {
            Pet pet = new Dog();
        }
    }
}
