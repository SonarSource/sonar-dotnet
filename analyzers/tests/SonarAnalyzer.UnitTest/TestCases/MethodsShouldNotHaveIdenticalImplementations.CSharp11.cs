using System;

namespace Tests.Diagnostics
{
    interface IInterface
    {
        static virtual void First()
//                          ^^^^^ Secondary
        {
            string s = "test";
            Console.WriteLine("Result: {0}", s);
        }

        static virtual void Second() // Noncompliant {{Update this method so that its implementation is not identical to 'First'.}}
//                          ^^^^^^
        {
            string s = "test";
            Console.WriteLine("Result: {0}", s);
        }

        static virtual void Different()
        {
            string s = "this is a different method";
            Console.WriteLine("Result: {0}", s);
        }
    }
}
