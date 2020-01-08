using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    abstract class Base
    {
        public virtual void Method(int[] numbers)
        {
        }
        public virtual void Method(string s,
            params int[] numbers)
        {
        }
        public abstract void Method(string s, string s1,
            int[] numbers);

        public virtual void Method(string s, string s1, string s2,
            params int[] numbers)
        {
        }
    }
    abstract class Derived : Base
    {
        public override void Method(params int[] numbers) // Noncompliant {{'params' should be removed from this override.}}
//                                  ^^^^^^
        {
        }
        public override void Method(string s,
            params int[] numbers)
        {
        }
        public override void Method(string s, string s1,
            params int[] numbers) // Noncompliant
        { }

        public override void Method(string s, string s1, string s2,
            int[] numbers)
        {
        }
    }

    public interface IFirst
    {
        public virtual void Method(int[] numbers)
        {
        }
    }

    public class Second : IFirst
    {
        public void Method(params int[] numbers) // Compliant - The interface method can be accessed only after cast (due to the fact that it has an implementation).
        {
        }
    }
}
