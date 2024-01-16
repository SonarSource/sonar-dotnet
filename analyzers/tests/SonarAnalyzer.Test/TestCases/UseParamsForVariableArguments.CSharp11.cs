using System;
using System.Runtime.InteropServices;

namespace Tests.Diagnostics
{
    public interface IFoo
    {
        static abstract void Foo(__arglist); // Noncompliant
    }

    public class BaseClass : IFoo
    {
        public static void Foo(__arglist) // Compliant - interface implementation
        {
        }

        public virtual void Do(__arglist) // Noncompliant
        {
        }
    }
}
