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
        public override void Method(params int[] numbers) // Fixed
        {
        }
        public override void Method(string s,
            params int[] numbers) // Fixed
        {
        }
        public override void Method(string s, string s1,
            params int[] numbers) // Fixed
        { }
    }

    abstract class Derived2 : Base
    {
        public override void Method(params int[] numbers)
        {
        }
    }
}
