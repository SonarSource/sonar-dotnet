using System;
using System.Collections;
using System.IO;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public class NonGeneric
    {

    }

    public interface Interface
    {
        int Add<T>(int a, int b); //Compliant
    }

    public class InterfaceImplementation : Interface, IDummyInterface // Error [CS0246] - IDummyInterface not found
    {
        public int Add<T>(int a, int b) //Compliant, it is implementing the interface.
        {
            return 0;
        }

        int IDummyInterface.MyMethod<T>(int a, int b) // Error [CS0246,CS0538] - Compliant, it is implementing the interface, although we don't know anything about it
                                                      // Ignore@-1 CS0538
        {
            return 0;
        }
    }

    public class MoreMath<T> // Noncompliant {{'T' is not used in the class.}}
//                        ^
    {
        public int Add<T>(int a, int b) // Noncompliant; <T> is ignored
        {
            return a + b;
        }
    }

    public class MoreMath2<T> : List<T>
    {
        public T Property { get; set; }
        public int Add<T>(int a, int b) // Noncompliant; <T> is ignored
        {
            return a + b;
        }

        public int Substract<T>(int a, int b) => a - b; // Noncompliant
    }
    public class MoreMath3<T> : MoreMath2<T>
    {
    }

    public class MoreMath4<T, T3> : MoreMath2<T> // Noncompliant
    {
    }

    public class MoreMath5<T, T3> : MoreMath2<Dictionary<string, List<T>>>
    {
        public List<T3> DoStuff<T3>(List<T3> o)
        {
            return o;
        }

        public T3 DoStuff<T, T3>(params T3[] o) // Noncompliant
        {
            return o[0];
        }
    }

    public abstract class MoreMath
    {
        public int Add(int a, int b)
        {
            return a + b;
        }

        public abstract int Do<T>(int a);
    }

    public class ComplexMath : MoreMath
    {
        public override int Do<T>(int a)
        {
            //don't use T here, but the method is still compliant because it is an override
            return 1;
        }
    }

    public partial class SomeClass<T>
    {
        private class Inner : List<T>
        {
        }
    }

    public partial class SomeClass<T>
    {
    }

    public class MyNotReallyGenericClass<T> // Noncompliant
        where T : class
    {

    }
    public class MyCompliantSpecialGenericClass<T1, T2> // Compliant
        where T1 : class
        where T2 : T1
    {
        public void MyMethod(T2 p) { }
    }
    public class MyNonCompliantSpecialGenericClass<
        T1, // Compliant, not recognized that it's a non used type parameter
        T2> // Noncompliant
        where T1 : class
        where T2 : T1
    {
    }
}
