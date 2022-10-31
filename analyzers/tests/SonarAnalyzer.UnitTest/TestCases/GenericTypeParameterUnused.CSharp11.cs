using System;

namespace Tests.Diagnostics
{

    public interface Interface
    {
        static abstract int AddAbstract<T>(int a, int b); //Compliant

        static virtual int AddVirtual<T>(int a, int b) // FN (<T> is ignored)
        {
            return a + b;
        }
    }

    public class InterfaceImplementation : Interface
    {
        public static int AddAbstract<T>(int a, int b) //Compliant, it is implementing the interface.
        {
            return 0;
        }
    }
}
