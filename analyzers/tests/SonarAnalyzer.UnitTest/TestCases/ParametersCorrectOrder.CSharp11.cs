using System;

namespace Tests.Diagnostics
{
    public interface IInterface
    {
        static virtual void SomeMethod(int a, int b) { } // Secondary
    }

    public class SomeClass<T> where T : IInterface
    {
        public SomeClass()
        {
            int a = 1;
            int b = 2;

            T.SomeMethod(b, a); // Noncompliant
        }
    }
}
