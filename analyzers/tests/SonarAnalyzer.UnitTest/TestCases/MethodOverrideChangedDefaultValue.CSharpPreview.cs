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
        public static void Write(int i, int j = 0) // Noncompliant {{Use the default parameter value defined in the overridden method.}}
//                                              ^
        {
            Console.WriteLine(i);
        }
    }
}
