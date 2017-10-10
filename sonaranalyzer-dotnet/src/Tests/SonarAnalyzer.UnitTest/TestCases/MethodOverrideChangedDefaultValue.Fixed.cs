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
        public virtual void Write(int i, int j = 5) // Fixed
        {
            Console.WriteLine(i);
        }
    }

    public class Derived1 : Base
    {
        public override void Write(int i,
            int j = 5) // Fixed
        {
            Console.WriteLine(i);
        }
    }

    public class Derived2 : Base
    {
        public override void Write(int i,
            int j = 5) // Fixed
        {
            Console.WriteLine(i);
        }
    }

    public class Derived3 : Base
    {
        public override void Write(int i,  // Fixed
            int j = 5)
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
}
