using System;
using System.CodeDom.Compiler;

namespace SomeNamespace
{
    class Class_With_Local_Function
    {
        static void Method(string[] args)
        {
            object obj = null;
            Console.WriteLine(obj.ToString());

            [GeneratedCodeAttribute("foo", "bar")]
            void Foo()
            {
            }
        }
    }
}
