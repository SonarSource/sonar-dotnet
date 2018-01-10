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
        public virtual void Write(int i, int j = 0) // Noncompliant {{Use the default parameter value defined in the overridden method.}}
//                                               ^
        {
            Console.WriteLine(i);
        }
    }

    public class Derived1 : Base
    {
        public override void Write(int i,
            int j = 42) // Noncompliant
        {
            Console.WriteLine(i);
        }
    }

    public class Derived2 : Base
    {
        public override void Write(int i,
            int j) // Noncompliant
        {
            Console.WriteLine(i);
        }
    }

    public class Derived3 : Base
    {
        public override void Write(int i = 5,  // Noncompliant {{Remove the default parameter value to match the signature of overridden method.}}
            int j = 0)
        {
            Console.WriteLine(i);
        }
    }

    public class ExplicitImpl1 : IMyInterface
    {
        void IMyInterface.Write(int i,
            int j = 0) // Noncompliant
        {
        }
    }
}
