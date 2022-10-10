using System;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tests.Diagnostics
{

    public interface Interface
    {
        static abstract int Add<T>(int a, int b); //Compliant
    }

    public class InterfaceImplementation : Interface
    {
        public static int Add<T>(int a, int b) //Compliant, it is implementing the interface.
        {
            return 0;
        }
    }
}
