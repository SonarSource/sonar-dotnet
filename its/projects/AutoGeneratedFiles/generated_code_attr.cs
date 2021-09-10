using System;
using System.CodeDom.Compiler;

namespace SomeNamespace
{
    class Class_12
    {
        static void Method(string[] args)
        {
            object obj = null;
            Console.WriteLine(obj.ToString());
        }

        [GeneratedCode("foo", "bar")]
        private void Foo()
        {
        }
    }
}
