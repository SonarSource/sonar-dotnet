using System;

namespace Net6Poc
{
    public interface ISomeInterface
    {
        static abstract void SomeMethod1();
    }

    public class SomeClass: ISomeInterface
    {
        public static async void SomeMethod1() // Compliant as it comes from the interface
        {
            return;
        }

        public static async void SomeMethod2() // Noncompliant
        {
            return;
        }
    }
}
