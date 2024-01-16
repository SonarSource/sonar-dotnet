using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    delegate int Deleg<T>(List<T> obj, T obj2);
    delegate int Deleg2<T>(IEnumerable<T> obj, T obj2); //Noncompliant
//                      ^
    delegate int Deleg2Ok<in T>(IEnumerable<T> obj, T obj2);
    delegate IEnumerable<T> Deleg3<T>(); //Noncompliant
    delegate T Deleg4<out T>();
    delegate Action<T> Deleg5<T>(); //Noncompliant
    delegate T Deleg5<T, U>(int i, int j); //Noncompliant

    interface IConsumer<T> // Noncompliant {{Add the 'in' keyword to parameter 'T' to make it 'contravariant'.}}
    {
        bool Eat(T fruit);
        void Eat2(T fruit, T fruit1, int fruit2);
        T Prop { set; }
    }

    interface IConsumerOk<in T>
    {
        bool Eat(T fruit);
        void Eat2(T fruit, T fruit1, int fruit2);
        T Prop { set; }
    }

    public delegate void SomeEventDelegate<in T>(object sender, T data);
    interface IConsumer2<T> // Noncompliant
    {
        event SomeEventDelegate<T> SomeEvent;
        T Eat();
    }

    interface IConsumer3<T> // Noncompliant
    {
        T M();
        T Prop { get; }
    }

    interface IConsumer4<T> //Noncompliant
    {
        IEnumerable<T> M();
    }
    interface IConsumer5<T> //we don't report if it is not used
    {
        void M();
    }
    interface IException<T>
    {
        void M(ref T p);
    }
    interface IException2<T>
    {
        void M(out T p);
    }
}
