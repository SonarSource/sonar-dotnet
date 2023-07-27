using System;

namespace MyLibrary
{
    public interface ISomeMethodsInterface
    {
        void Method();
    }

    public interface MyInterface { } // Noncompliant {{Remove this interface or add members to it.}}
//                   ^^^^^^^^^^^

    public interface MyInterface2 : ISomeMethodsInterface { } // Noncompliant

    public interface MyInterface3
    {
        void Foo();
    }

    public interface MyInterface4
    {
        bool Bar { get; }
    }

    internal interface MyInterfaceInternal { }

    interface MyInterfaceDefault { }

    public class Container
    {
        public interface IPublic { } // Noncompliant
        private interface IPrivate { }
        internal interface IInternal { }
    }

    // This is not a marker interface, it aggregates other interfaces and is ok not to have members
    interface MyInterface5 : ISomeMethodsInterface, MyInterface3 // Compliant
    {
    }

    public interface // Error [CS1001]
    {
    }

    public interface IGeneric<T> { }                                                 // Noncompliant
    public interface IBoundGeneric : IGeneric<int> { }                               // Compliant: Specilized version
    public interface IUnboundGeneric<T> : IGeneric<T> { }                            // Noncompliant: Just an alias of the base interface
    public interface IUnboundConstraintGeneric<T> : IGeneric<T> where T : struct { } // Compliant: specialized version
}
