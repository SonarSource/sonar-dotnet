using System;
using System.Runtime.InteropServices;

namespace Tests.Diagnostics
{
    public class BaseClass(__arglist)
    {
        public void Foo()
        {
            ArgIterator argumentIterator = new ArgIterator(__arglist); // Error CS0190
            for (int i = 0; i < argumentIterator.GetRemainingCount(); i++)
            {
                Console.WriteLine(__refvalue(argumentIterator.GetNextArg(), string));
            }
        }
    }
}
