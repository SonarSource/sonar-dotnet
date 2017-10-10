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
        public override void Method(int[] numbers) // Fixed
        {
        }
        public override void Method(string s,
            params int[] numbers)
        {
        }
        public override void Method(string s, string s1,
            int[] numbers) // Fixed
        { }

        public override void Method(string s, string s1, string s2,
            int[] numbers)
        {
        }
    }
}
