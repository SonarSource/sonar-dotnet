using System;

// this file does not get ignored
namespace SomeNamespace
{
    class ClassWrittenByHand
    {
        static void Method(string[] args)
        {
            object obj = null;
            Console.WriteLine(obj.ToString());
        }

        private void Foo()
        {
        }
    }
}
