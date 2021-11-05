using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public interface IMyInterface
    {
        static abstract void Write(int i, int j = 5);
    }

    public class Class : IMyInterface
    {
        public static void Write(int i, int j = 5) // Fixed
        {
            Console.WriteLine(i);
        }
    }
}
