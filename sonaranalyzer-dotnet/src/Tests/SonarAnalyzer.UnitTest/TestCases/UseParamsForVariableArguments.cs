using System;

namespace Tests.Diagnostics
{
    public class Program
    {
        public Program() { }

        public void Bar(__arglist) // Noncompliant
//                  ^^^
        {
            ArgIterator argumentIterator = new ArgIterator(__arglist);
            for (int i = 0; i < argumentIterator.GetRemainingCount(); i++)
            {
                Console.WriteLine(
                    __refvalue(argumentIterator.GetNextArg(), string));
            }
        }

        protected void Bar2(__arglist) // Noncompliant
        {
            ArgIterator argumentIterator = new ArgIterator(__arglist);
            for (int i = 0; i < argumentIterator.GetRemainingCount(); i++)
            {
                Console.WriteLine(
                    __refvalue(argumentIterator.GetNextArg(), string));
            }
        }

        public void Bar3(int val, string name, __arglist) // Noncompliant
        {
            ArgIterator argumentIterator = new ArgIterator(__arglist);
            for (int i = 0; i < argumentIterator.GetRemainingCount(); i++)
            {
                Console.WriteLine(
                    __refvalue(argumentIterator.GetNextArg(), string));
            }
        }

        private class Foo
        {
            public void Bar4(__arglist) // Compliant - private method
            {
                ArgIterator argumentIterator = new ArgIterator(__arglist);
                for (int i = 0; i < argumentIterator.GetRemainingCount(); i++)
                {
                    Console.WriteLine(
                        __refvalue(argumentIterator.GetNextArg(), string));
                }
            }
        }
    }
}
