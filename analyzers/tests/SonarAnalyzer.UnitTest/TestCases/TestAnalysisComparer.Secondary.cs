using System;

namespace Tests.Diagnostics
{
    public class SecondClass
    {
        async void WrongLength() { } // Noncompliant {{Return 'Task' instead.}}
        //    ^^^^^

        void Default() // Secondary [flow-0]
                       // Secondary@-1 [flow-1] {{Wrong message}}
        {
            string s = "test";
            Console.WriteLine("Result: {0}", s); // Secondary [flow-2] - Wrong secondary with missing primary issue
        }

        void WrongMessage() // Noncompliant [flow-0] {{Wrong message}}
        {
            string s = "test";
            Console.WriteLine("Result: {0}", s);
        }

        void CorrectNoncompliant() // Noncompliant [flow-1] {{Update this method so that its implementation is not identical to 'Default'.}}
        {
            string s = "test";
            Console.WriteLine("Result: {0}", s);
        }
    }
}
