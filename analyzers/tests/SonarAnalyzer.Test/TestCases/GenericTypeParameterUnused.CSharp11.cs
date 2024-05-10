using System;

namespace Tests.Diagnostics
{

    public interface Interface
    {
        static int AddStatic<T>(int a, int b)               // Noncompliant
        {
            return a + b;
        }

        static abstract int AddAbstract<T>(int a, int b);   // Compliant

        static virtual int AddVirtual<T>(int a, int b)      // Compliant: T might be used in implementation of the interface
        {
            return a + b;
        }
    }

    public class InterfaceImplementation : Interface
    {
        public static int AddAbstract<T>(int a, int b)      // Compliant: it is implementing the interface.
        {
            return 0;
        }
    }
}
