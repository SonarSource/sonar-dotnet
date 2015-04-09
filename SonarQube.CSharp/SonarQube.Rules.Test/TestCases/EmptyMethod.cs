using System;
using System.Diagnostics;

namespace Tests.Diagnostics
{
    public class EmptyMethod
    {
        private EmptyMethod()
        {
        }

        void F2()
        {
           // Do nothing because of X and Y.
        }

        void F3()
        {
          Console.WriteLine();
        }

        [Conditional("DEBUG")]
        void F4()    // Noncompliant
        {
        }

        protected virtual void F5()
        {
        }

        extern void F6();
    }

    public abstract class MyClass
    {
        public void F1() // Noncompliant
        {
        }

        public abstract void F2();
    }
}
