using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    abstract class Base
    {
        public virtual void Method(params int[] numbers)
        {
        }
        public virtual void Method(string s,
            params int[] numbers)
        {
        }
        public abstract void Method(string s, string s1,
            params int[] numbers);
    }
    abstract class Derived : Base
    {
        public override void Method(int[] numbers) // Noncompliant {{'params' should not be removed from an override.}}
//                                  ^^^^^^^^^^^^^
        {
        }
        public override void Method(string s,
            int[] numbers) // Noncompliant
        {
        }
        public override void Method(string s, string s1,
            int[] numbers) // Noncompliant
        { }
    }

    abstract class Derived2 : Base
    {
        public override void Method(params int[] numbers)
        {
        }
    }
}
