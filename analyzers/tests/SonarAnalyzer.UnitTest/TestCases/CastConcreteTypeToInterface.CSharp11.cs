using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    file interface IMyInterface
    {
        void DoStuff();
    }

    file class MyClass1 : IMyInterface
    {
        public void DoStuff()
        {
            // TODO...
        }
    }

    file class DowncastExampleProgram
    {
        static void EntryPoint(IMyInterface interfaceRef)
        {
            MyClass1 class1 = (MyClass1)interfaceRef;  // Noncompliant {{Remove this cast and edit the interface to add the missing functionality.}}
//                            ^^^^^^^^^^^^^^^^^^^^^^

            class1 = interfaceRef as MyClass1;  // Noncompliant
//                   ^^^^^^^^^^^^^^^^^^^^^^^^
        }
    }
}
