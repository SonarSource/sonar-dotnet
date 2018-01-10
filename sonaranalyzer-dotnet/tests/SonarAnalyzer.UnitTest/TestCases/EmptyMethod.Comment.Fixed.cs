using System;
using System.Diagnostics;

namespace Tests.Diagnostics
{
    public class EmptyMethod
    {
        void F2()
        {
            // Do nothing because of X and Y.
        }

        void F3()
        {
            Console.WriteLine();
        }

        [Conditional("DEBUG")]
        void F4()    // Fixed
        {
            // Method intentionally left empty.
        }

        protected virtual void F5()
        {
        }

        extern void F6();
    }

    public abstract class MyClass
    {
        public void F1()
        {
            // Method intentionally left empty.
        } // Fixed

        public abstract void F2();
    }

    public class MyClass5 : MyClass
    {
        public override void F2()
        {
        }
    }
}
