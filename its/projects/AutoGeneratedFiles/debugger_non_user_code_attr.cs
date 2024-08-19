using System;
using System.Diagnostics;

namespace SomeNamespace
{
    class Class_11
    {
        static void Method(string[] args)
        {
            object obj = null;
            Console.WriteLine(obj.ToString());
        }

        [DebuggerNonUserCodeAttribute]
        private void Foo()
        {
        }
    }
}
