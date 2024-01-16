using System;
using System.Runtime.InteropServices;

namespace Tests.Diagnostics
{
    public interface IFoo
    {
        void Foo(__arglist); // Noncompliant
    }

    public class BaseClass : IFoo
    {
        public void Foo(__arglist) // Compliant - interface implementation
        {
        }

        public virtual void Do(__arglist) // Noncompliant
        {
        }
    }

    public class Program : BaseClass
    {
        public Program(__arglist) { } // Noncompliant {{Use the 'params' keyword instead of '__arglist'.}}

        public void Bar(__arglist) // Noncompliant {{Use the 'params' keyword instead of '__arglist'.}}
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

        public override void Do(__arglist) // Compliant - override
        {
        }

        [DllImport("msvcrt40.dll")]
        public static extern int printf(string format, __arglist); // Compliant - interop

        public void Bar4()
        {
            printf("Hello %s!\n", __arglist("Bart"));
        }

        private class FooBar
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
