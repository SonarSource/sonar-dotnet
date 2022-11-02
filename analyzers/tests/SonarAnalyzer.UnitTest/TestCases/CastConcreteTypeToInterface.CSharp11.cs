using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    file interface IMyInterface
    {
        void DoStuff();
    }
    file interface IMyInterface2
    {
    }

    file class MyClass1 : IMyInterface
    {
        public int Data { get { return new Random().Next(); } }

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
            int privateData = class1.Data;

            class1 = interfaceRef as MyClass1;  // Noncompliant
//                   ^^^^^^^^^^^^^^^^^^^^^^^^
            if (class1 != null)
            {
                // ...
            }

            var interf = (IMyInterface2)interfaceRef;
            interf = (IMyInterface2)class1;
            var o = (object)interfaceRef;

            IEnumerable<int> list = null;
            var l = (List<int>)list; // Compliant
        }
    }
}
