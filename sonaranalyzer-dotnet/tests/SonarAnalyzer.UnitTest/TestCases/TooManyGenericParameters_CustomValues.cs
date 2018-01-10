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

        public void Foo<T1, T2, T3, T4>()
        {
        }

        public void Foo<T1, T2, T3, T4, T5>() // Noncompliant {{Reduce the number of generic parameters in the 'Bar.Foo' method to no more than the 4 authorized.}}
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

    class Bar<T1, T2, T3>
    {
    }

    class Bar<T1, T2, T3, T4>
    {
    }

    class Bar<T1, T2, T3, T4, T5> // Noncompliant {{Reduce the number of generic parameters in the 'Bar' class to no more than the 4 authorized.}}
//        ^^^
    {
    }
}
