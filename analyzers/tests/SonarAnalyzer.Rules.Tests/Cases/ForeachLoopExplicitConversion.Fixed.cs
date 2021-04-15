using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Tests.Diagnostics
{
    interface I { }
    class A : I { }
    class B : A { }
    class ForeachLoopExplicitConversion
    {
        public void S(string s)
        {
            foreach (var item in s)
            { }
            foreach (int item in s)
            { }
            foreach (A item in s) // Compliant // Error [CS0030] - cannot convert
            { }
        }
        public void M1(IEnumerable<int> enumerable)
        {
            foreach (int i in enumerable)
            { }
            foreach (var i in enumerable)
            { }
            foreach (object i in enumerable)
            { }
        }
        public void M2(IEnumerable<A> enumerable)
        {
            foreach (A i in enumerable)
            { }
            foreach (var i in enumerable)
            { }
            foreach (B i in enumerable.OfType<B>()) // Fixed
            { }
        }
        public void M3(A[] array)
        {
            foreach (A i in array)
            { }
            foreach (I i in array)
            { }
            foreach (B i in array.OfType<B>()) // Fixed
            { }
        }
        public void M4(A[][] array)
        {
            foreach (A[] i in array)
            { }
            foreach (object[] i in array)
            { }
            foreach (var i in array)
            { }
            foreach (B[] i in array.OfType<B[]>()) // Fixed
            { }
        }
        public void M5(ArrayList list)
        {
            foreach (A i in list)
            { }
            foreach (var i in list)
            { }
            foreach (object i in list)
            { }
            foreach (B i in list)
            { }
        }
    }

    public interface IMyInterface
    { }

    public class Base
    {

    }
    public class Derived : Base, IMyInterface
    { }

    public class OtherType
    {
        public static implicit operator OtherType(Derived self)
        {
            return null;
        }
    }

    class MyTest
    {
        public void Test()
        {
            foreach (Derived x in new Base[12].OfType<Derived>()) { } // Fixed
            foreach (Derived x in new object[12]) { }

            foreach (Derived x in new List<Base>().OfType<Derived>()) { } // Fixed
            foreach (Derived x in new List<object>()) { }
            foreach (Base x in new List<Derived>()) { }
            foreach (Derived x in new List<IMyInterface>().OfType<Derived>()) { } // Fixed

            foreach (Derived x in new ArrayList()) { }
            //We decided to not add the necessary complexity to recognize the following corner case
            foreach (OtherType x in new List<Base>()) { } // Compliant, although it can throw

            foreach (OtherType x in new List<Derived>()) { }
        }
    }
}
