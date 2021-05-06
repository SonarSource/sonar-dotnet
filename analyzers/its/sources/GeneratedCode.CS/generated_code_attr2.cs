using System;
using System.CodeDom.Compiler;

namespace SomeNamespace
{
    class Class_13
    {
        static void Method(string[] args)
        {
            object obj = null;
            Console.WriteLine(obj.ToString());
        }

        [GeneratedCodeAttribute("foo", "bar")]
        private void Foo()
        {
        }
    }
}
