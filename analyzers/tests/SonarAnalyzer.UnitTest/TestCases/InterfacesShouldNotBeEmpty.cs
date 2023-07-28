using System;
using System.Collections;
using System.Collections.Generic;

namespace MyLibrary
{
    public interface ISomeMethodsInterface
    {
        void Method();
    }

    public interface MyInterface { } // Noncompliant {{Remove this interface or add members to it.}}
    //               ^^^^^^^^^^^

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

    public interface ISortedCollection<T> : ICollection<T> { }  // Noncompliant: ICollection<T> is derived from a lot other interfaces but this is not "aggregation" like MyInterface5
    public interface ISortedList : IList { }                    // Noncompliant: Same here, still just a marker interface

    public interface // Error [CS1001]
    {
    }

    public interface IGeneric<T> { }                                                       // Noncompliant
    public interface IBoundGeneric : IGeneric<int> { }                                     // Compliant: specialized version
    public interface IUnboundGeneric<T> : IGeneric<T> { }                                  // Noncompliant: Just an alias of the base interface
    public interface IUnboundConstraintGeneric1<T> : IGeneric<T> where T : struct { }      // Compliant: specialized version, constraint struct
    public interface IUnboundConstraintGeneric2<T> : IGeneric<T> where T : class { }       // Compliant: specialized version, constraint class
    public interface IUnboundConstraintGeneric3<T> : IGeneric<T> where T : new() { }       // Compliant: specialized version, constraint new
    public interface IUnboundConstraintGeneric4<T> : IGeneric<T> where T : IDisposable { } // Compliant: specialized version, constraint type
    public interface IGeneric2_Constraint1<T1, T2> : IGeneric<T1> where T1 : new() { }     // Compliant: specialized version
    public interface IGeneric2_Constraint2<T1, T2> : IGeneric<T1> where T2 : new() { }     // Compliant: FN. Not a specialization of the base type parameter
    public interface IGeneric3<T> where T : new() { }                                      // Noncompliant: No base interface

    [Obsolete("Interface with attribute")]
    public interface Attributed1 { }                  // Noncompliant: An interface with an attribute is still only usefull as a marker
                                                      // Note: Implementing types do not inherit this attribute even if AttributeUsageAttribute.Inherited = true

    [Obsolete("Interface with attribute")]
    public interface Attributed2 : MyInterface { }    // Compliant: A derived interface with attribute enhances the base interface

    [Obsolete("Interface with attribute")]
    public interface Attributed3<T> : IGeneric<T> { } // Compliant: A derived interface with attribute enhances the base interface
}
