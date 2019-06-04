using System;
using System.Diagnostics;

namespace SomeNamespace
{
    class Class_10
    {
        static void Method(string[] args)
        {
            object obj = null;
            Console.WriteLine(obj.ToString());
        }

        [DebuggerNonUserCode]
        private void Foo()
        {
        }
    }
}
