using System;

namespace Tests.Diagnostics
{
    public interface IContract
    {
        static abstract void Do();
    }

    internal class SomeClass: IContract
    {
        public static void Do()
        {
            throw new NotImplementedException(); // Compliant because comes from the interface
        }
    }
}
