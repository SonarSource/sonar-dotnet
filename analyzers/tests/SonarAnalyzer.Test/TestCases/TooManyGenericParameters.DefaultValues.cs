using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    class Bar
    {
        public void Foo()
        {
        }

        public void Foo<T>()
        {
        }

        public void Foo<T1, T2>()
        {
        }

        public void Foo<T1, T2, T3>()
        {
        }

        public void Foo<T1, T2, T3, T4>() // Noncompliant  {{Reduce the number of generic parameters in the 'Bar.Foo' method to no more than the 3 authorized.}}
//                  ^^^
        {
        }

        public void Foo<T1, T2, T3, T4, T5>() // Noncompliant
//                  ^^^
        {
        }
    }

    class Bar<T>
    {
    }

    class Bar<T1, T2>
    {
    }

    class Bar<T1, T2, T3> // Noncompliant {{Reduce the number of generic parameters in the 'Bar' class to no more than the 2 authorized.}}
//        ^^^
    {
    }

    class Bar<T1, T2, T3, T4> // Noncompliant
//        ^^^
    {
    }

    class Bar<T1, T2, T3, T4, T5> // Noncompliant
//        ^^^
    {
    }

    struct Str<T1, T2, T3> // Noncompliant {{Reduce the number of generic parameters in the 'Str' struct to no more than the 2 authorized.}}
//         ^^^
    {
        public void Foo<T1, T2, T3>() { }

        public void Foo<T1, T2, T3, T4>() { } // Noncompliant
    }

    interface IFoo<T1, T2, T3> // Noncompliant {{Reduce the number of generic parameters in the 'IFoo' interface to no more than the 2 authorized.}}
//            ^^^^
    {
        void Foo<T1, T2, T3>();

        void Foo<T1, T2, T3, T4>(); // Noncompliant
//           ^^^
    }
}

namespace MyLib
{
    public abstract class FrameworkBaseClass<T1, T2, T3> // Noncompliant
    {
        void Method()
        {
            bool GenericLambda<T1, T2, T3, T4>() => true; // Noncompliant
        }
    }
}

namespace TheProject
{
    using MyLib;

    public class Impl : FrameworkBaseClass<int, double, bool>
    {

    }
}
