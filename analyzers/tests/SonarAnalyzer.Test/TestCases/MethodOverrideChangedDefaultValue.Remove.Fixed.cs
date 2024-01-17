using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public interface IMyInterface
    {
        void Write(int i, int j = 5);
    }

    public class Base : IMyInterface
    {
        public virtual void Write(int i, int j = 0) // Fixed
        {
            Console.WriteLine(i);
        }
    }

    public class Derived1 : Base
    {
        public override void Write(int i,
            int j = 42) // Fixed
        {
            Console.WriteLine(i);
        }
    }

    public class Derived2 : Base
    {
        public override void Write(int i,
            int j) // Fixed
        {
            Console.WriteLine(i);
        }
    }

    public class Derived3 : Base
    {
        public override void Write(int i = 5,  // Fixed
            int j = 0)
        {
            Console.WriteLine(i);
        }
    }

    public class ExplicitImpl1 : IMyInterface
    {
        void IMyInterface.Write(int i,
            int j) // Fixed
        {
        }
    }

    public interface IFirst
    {
        void Write(int i, int j = 5);

        void Write(int i = 42);
    }

    public interface ISecond : IFirst
    {
        void Write(int i, int j = 5)
        {
        }

        void Write(int i = 0) // Compliant - This method can be called only after a cast to ISecond
        {
        }
    }
}
