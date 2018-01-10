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
            throw new NotSupportedException();
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
            throw new NotSupportedException();
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
