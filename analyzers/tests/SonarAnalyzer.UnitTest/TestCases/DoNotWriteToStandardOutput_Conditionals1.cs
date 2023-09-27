#define DEBUG
#define debug
#define BLOCK1

using System;

namespace Tests.Diagnostics
{
    class Tests
    {

#if DEBUG
        public void Method1()
        {
            Console.WriteLine("Hello World"); // compliant - in a debug section
        }
#else
        public void DoStuff()
        {
            Console.WriteLine("Hello World"); // won't be processed - nodes aren't active
        }
#endif

#if debug
        public void DoStuff()
        {
            Console.WriteLine("Hello World");  // Noncompliant
        }
#endif

#if BLOCK1
        public void Method2()
        {
            Console.WriteLine("Hello World"); // Noncompliant
        }
#endif
    }
}
